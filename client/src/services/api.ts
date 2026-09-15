import axios, { type InternalAxiosRequestConfig, type AxiosError } from 'axios';

interface FailedRequestQueueItem {
    resolve: (token: string) => void;
    reject: (error: unknown) => void;
}

const api = axios.create({
    baseURL: import.meta.env.VITE_API_BASE_URL || '/api',
});

let isRefreshing = false;
let failedQueue: FailedRequestQueueItem[] = [];

const processQueue = (error: unknown, token: string | null = null) => {
    failedQueue.forEach((item) => {
        if (error) {
            item.reject(error);
        } else if (token) {
            item.resolve(token);
        }
    });
    failedQueue = [];
};

const isAuthEndpoint = (url?: string): boolean => {
    if (!url) return false;
    return (
        url.includes('/auth/login') ||
        url.includes('/auth/register') ||
        url.includes('/auth/refresh') ||
        url.includes('/auth/logout')
    );
};

api.interceptors.request.use(
    (config: InternalAxiosRequestConfig) => {
        const token = localStorage.getItem('token');
        if (token && token !== 'undefined' && token !== 'null' && config.headers) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
);

api.interceptors.response.use(
    (response) => response,
    async (error: AxiosError) => {
        const originalRequest = error.config as (InternalAxiosRequestConfig & { _retry?: boolean }) | undefined;

        // If no request config or not a 401 error, pass through
        if (!originalRequest || !error.response || error.response.status !== 401) {
            return Promise.reject(error);
        }

        // Do not attempt refresh on authentication endpoints themselves
        if (isAuthEndpoint(originalRequest.url)) {
            return Promise.reject(error);
        }

        // If the request was already retried via refresh, avoid infinite loops
        if (originalRequest._retry) {
            localStorage.removeItem('token');
            localStorage.removeItem('refreshToken');
            localStorage.removeItem('user');
            if (window.location.pathname !== '/login') {
                window.location.href = '/login';
            }
            return Promise.reject(error);
        }

        // If a refresh request is already in progress, queue this request
        if (isRefreshing) {
            return new Promise<string>((resolve, reject) => {
                failedQueue.push({ resolve, reject });
            })
                .then((newToken) => {
                    if (originalRequest.headers) {
                        originalRequest.headers.Authorization = `Bearer ${newToken}`;
                    }
                    return api(originalRequest);
                })
                .catch((err) => Promise.reject(err));
        }

        originalRequest._retry = true;
        isRefreshing = true;

        const storedRefreshToken = localStorage.getItem('refreshToken');
        if (!storedRefreshToken) {
            isRefreshing = false;
            processQueue(error, null);
            localStorage.removeItem('token');
            localStorage.removeItem('refreshToken');
            localStorage.removeItem('user');
            if (window.location.pathname !== '/login') {
                window.location.href = '/login';
            }
            return Promise.reject(error);
        }

        try {
            const baseURL = import.meta.env.VITE_API_BASE_URL || '/api';
            // Use plain axios.post to prevent triggering this interceptor recursively
            const refreshResponse = await axios.post<{
                accessToken?: string;
                token?: string;
                refreshToken?: string;
                user?: unknown;
            }>(
                `${baseURL}/auth/refresh`,
                { refreshToken: storedRefreshToken },
                { headers: { 'Content-Type': 'application/json' } }
            );

            const { data } = refreshResponse;
            const newAccessToken = data.accessToken || data.token || '';
            const newRefreshToken = data.refreshToken || '';

            if (newAccessToken) {
                localStorage.setItem('token', newAccessToken);
                api.defaults.headers.common.Authorization = `Bearer ${newAccessToken}`;
            }

            if (newRefreshToken) {
                localStorage.setItem('refreshToken', newRefreshToken);
            }

            if (data.user) {
                localStorage.setItem('user', JSON.stringify(data.user));
            }

            processQueue(null, newAccessToken);

            if (originalRequest.headers && newAccessToken) {
                originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
            }

            return api(originalRequest);
        } catch (refreshError: any) {
            processQueue(refreshError, null);

            // Check if this is an explicit authentication rejection (e.g. 401 or 400)
            const isAuthFailure =
                refreshError.response &&
                (refreshError.response.status === 401 || refreshError.response.status === 400);

            if (isAuthFailure) {
                localStorage.removeItem('token');
                localStorage.removeItem('refreshToken');
                localStorage.removeItem('user');
                if (window.location.pathname !== '/login') {
                    window.location.href = '/login';
                }
            }

            return Promise.reject(refreshError);
        } finally {
            isRefreshing = false;
        }
    }
);

export default api;

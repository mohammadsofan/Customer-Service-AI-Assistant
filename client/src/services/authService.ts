import api from './api';

export interface User {
    id: string;
    username?: string;
    fullName?: string;
    email: string;
    role: string;
    isAdmin?: boolean;
    isActive?: boolean;
}

export interface AuthResponse {
    token: string;
    accessToken: string;
    refreshToken: string;
    user: User;
}

export interface LoginRequest {
    email: string;
    password?: string;
}

const authService = {
    login: async (credentials: LoginRequest): Promise<AuthResponse> => {
        const response = await api.post<any>('/auth/login', credentials);
        const data = response.data;
        const token = data.accessToken || data.token || '';
        const refreshToken = data.refreshToken || '';
        const user = data.user || {};
        const normalizedRole = (user.role || '').toLowerCase();
        const isAdmin = normalizedRole === 'admin' || normalizedRole === 'administrator';

        const normalizedUser: User = {
            ...user,
            username: user.fullName || user.username || user.email,
            fullName: user.fullName || user.username || user.email,
            isAdmin,
        };

        if (token) localStorage.setItem('token', token);
        if (refreshToken) localStorage.setItem('refreshToken', refreshToken);
        if (user) localStorage.setItem('user', JSON.stringify(normalizedUser));

        return {
            token,
            accessToken: token,
            refreshToken,
            user: normalizedUser
        };
    },
    refreshToken: async (token?: string, refreshToken?: string): Promise<AuthResponse> => {
        const currentRefreshToken = refreshToken || localStorage.getItem('refreshToken') || '';
        const currentToken = token || localStorage.getItem('token') || '';
        const response = await api.post<any>('/auth/refresh', { token: currentToken, refreshToken: currentRefreshToken });
        const data = response.data;
        const newToken = data.accessToken || data.token || '';
        const newRefreshToken = data.refreshToken || '';
        if (newToken) localStorage.setItem('token', newToken);
        if (newRefreshToken) localStorage.setItem('refreshToken', newRefreshToken);
        return {
            token: newToken,
            accessToken: newToken,
            refreshToken: newRefreshToken,
            user: data.user
        };
    },
    logout: async (): Promise<void> => {
        const storedRefreshToken = localStorage.getItem('refreshToken');
        if (storedRefreshToken) {
            try {
                await api.post('/auth/logout', { refreshToken: storedRefreshToken });
            } catch {
                // Ignore network errors to guarantee local state is cleared
            }
        }
        localStorage.removeItem('token');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('user');
    },
    getCurrentUser: (): User | null => {
        const userStr = localStorage.getItem('user');
        if (!userStr) return null;
        try {
            return JSON.parse(userStr);
        } catch {
            return null;
        }
    }
};

export default authService;

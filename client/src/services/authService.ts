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
        const user = data.user || {};
        const normalizedRole = (user.role || '').toLowerCase();
        const isAdmin = normalizedRole === 'admin' || normalizedRole === 'administrator';

        const normalizedUser: User = {
            ...user,
            username: user.fullName || user.username || user.email,
            fullName: user.fullName || user.username || user.email,
            isAdmin,
        };

        return {
            token,
            accessToken: token,
            refreshToken: data.refreshToken || '',
            user: normalizedUser
        };
    },
    refreshToken: async (token: string, refreshToken: string): Promise<AuthResponse> => {
        const response = await api.post<any>('/auth/refresh', { token, refreshToken });
        const data = response.data;
        const newToken = data.accessToken || data.token || '';
        return {
            token: newToken,
            accessToken: newToken,
            refreshToken: data.refreshToken || '',
            user: data.user
        };
    },
    logout: (): void => {
        localStorage.removeItem('token');
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

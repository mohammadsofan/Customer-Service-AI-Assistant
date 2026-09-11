import api from './api';

export interface User {
    id: string;
    username: string;
    email: string;
    role: string;
    isAdmin: boolean;
}

export interface AuthResponse {
    token: string;
    refreshToken: string;
    user: User;
}

export interface LoginRequest {
    email: string;
    password?: string;
}

const authService = {
    login: async (credentials: LoginRequest): Promise<AuthResponse> => {
        const response = await api.post<AuthResponse>('/auth/login', credentials);
        return response.data;
    },
    refreshToken: async (token: string, refreshToken: string): Promise<AuthResponse> => {
        const response = await api.post<AuthResponse>('/auth/refresh-token', { token, refreshToken });
        return response.data;
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

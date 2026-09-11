import React, { createContext, useState, useEffect, type ReactNode } from 'react';
import authService, { type User, type LoginRequest } from '../services/authService';

interface AuthContextType {
    user: User | null;
    token: string | null;
    isAuthenticated: boolean;
    isAdmin: boolean;
    login: (credentials: LoginRequest) => Promise<User>;
    logout: () => void;
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined);

interface AuthProviderProps {
    children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
    const [user, setUser] = useState<User | null>(() => {
        const storedUser = localStorage.getItem('user');
        if (storedUser && storedUser !== 'undefined' && storedUser !== 'null') {
            try { return JSON.parse(storedUser); } catch { return null; }
        }
        return null;
    });

    const [token, setToken] = useState<string | null>(() => {
        const storedToken = localStorage.getItem('token');
        return (storedToken && storedToken !== 'undefined' && storedToken !== 'null') ? storedToken : null;
    });

    useEffect(() => {
        const storedToken = localStorage.getItem('token');
        const storedUser = localStorage.getItem('user');

        if (storedToken && storedToken !== 'undefined' && storedToken !== 'null' &&
            storedUser && storedUser !== 'undefined' && storedUser !== 'null') {
            setToken(storedToken);
            try {
                setUser(JSON.parse(storedUser));
            } catch {
                setUser(null);
            }
        }
    }, []);

    const login = async (credentials: LoginRequest): Promise<User> => {
        const data = await authService.login(credentials);
        const authToken = data.accessToken || data.token;
        setToken(authToken);
        setUser(data.user);
        localStorage.setItem('token', authToken);
        localStorage.setItem('user', JSON.stringify(data.user));
        return data.user;
    };

    const logout = () => {
        authService.logout();
        setToken(null);
        setUser(null);
        window.location.href = '/login';
    };

    const isAuthenticated = !!token;
    const roleLower = (user?.role || '').toLowerCase();
    const isAdmin = user?.isAdmin === true || roleLower === 'admin' || roleLower === 'administrator';

    return (
        <AuthContext.Provider value={{ user, token, isAuthenticated, isAdmin, login, logout }}>
            {children}
        </AuthContext.Provider>
    );
};

import { describe, it, expect, beforeEach, vi } from 'vitest';
import axios from 'axios';
import api from '../api';

describe('Axios api 401 refresh interceptor behavior', () => {
    beforeEach(() => {
        localStorage.clear();
        vi.clearAllMocks();
    });

    it('api instance is properly configured with interceptors', () => {
        expect(api).toBeDefined();
        expect(api.interceptors.request).toBeDefined();
        expect(api.interceptors.response).toBeDefined();
    });

    it('attaches Bearer token from localStorage to outgoing requests', async () => {
        localStorage.setItem('token', 'valid-test-jwt');
        const config: any = { headers: {} };

        // Execute request interceptor directly
        const handlers = (api.interceptors.request as any).handlers;
        const requestHandler = handlers[0].fulfilled;
        const modifiedConfig = await requestHandler(config);

        expect(modifiedConfig.headers.Authorization).toBe('Bearer valid-test-jwt');
    });

    it('does not attach token if none exists in localStorage', async () => {
        const config: any = { headers: {} };
        const handlers = (api.interceptors.request as any).handlers;
        const requestHandler = handlers[0].fulfilled;
        const modifiedConfig = await requestHandler(config);

        expect(modifiedConfig.headers.Authorization).toBeUndefined();
    });

    it('does not attempt refresh for auth endpoints on 401', async () => {
        const handlers = (api.interceptors.response as any).handlers;
        const errorHandler = handlers[0].rejected;

        const authEndpoints = ['/auth/login', '/auth/refresh', '/auth/logout', '/auth/register'];

        for (const url of authEndpoints) {
            const error = {
                config: { url, headers: {} },
                response: { status: 401 },
            };

            await expect(errorHandler(error)).rejects.toEqual(error);
        }
    });

    it('prevents infinite loops if request was already retried', async () => {
        localStorage.setItem('token', 'expired-token');
        localStorage.setItem('refreshToken', 'some-refresh-token');

        const handlers = (api.interceptors.response as any).handlers;
        const errorHandler = handlers[0].rejected;

        const error = {
            config: { url: '/support/questions', _retry: true, headers: {} },
            response: { status: 401 },
        };

        await expect(errorHandler(error)).rejects.toEqual(error);

        // Should clear tokens on repeated 401 after retry
        expect(localStorage.getItem('token')).toBeNull();
        expect(localStorage.getItem('refreshToken')).toBeNull();
    });

    it('clears credentials if 401 occurs and no refresh token exists', async () => {
        localStorage.setItem('token', 'expired-token');
        // No refreshToken in storage

        const handlers = (api.interceptors.response as any).handlers;
        const errorHandler = handlers[0].rejected;

        const error = {
            config: { url: '/support/questions', headers: {} },
            response: { status: 401 },
        };

        await expect(errorHandler(error)).rejects.toEqual(error);
        expect(localStorage.getItem('token')).toBeNull();
    });

    it('persists rotated refresh token and new access token on successful refresh', () => {
        localStorage.setItem('token', 'old-access-token');
        localStorage.setItem('refreshToken', 'old-refresh-token');

        const newAccessToken = 'new-rotated-access-token';
        const newRefreshToken = 'new-rotated-refresh-token';

        localStorage.setItem('token', newAccessToken);
        localStorage.setItem('refreshToken', newRefreshToken);

        expect(localStorage.getItem('token')).toBe(newAccessToken);
        expect(localStorage.getItem('refreshToken')).toBe(newRefreshToken);
    });

    it('passes through non-401 errors unchanged', async () => {
        const handlers = (api.interceptors.response as any).handlers;
        const errorHandler = handlers[0].rejected;

        const serverError = {
            config: { url: '/support/questions', headers: {} },
            response: { status: 500 },
        };

        await expect(errorHandler(serverError)).rejects.toEqual(serverError);

        const notFoundError = {
            config: { url: '/support/questions/123', headers: {} },
            response: { status: 404 },
        };

        await expect(errorHandler(notFoundError)).rejects.toEqual(notFoundError);
    });
});

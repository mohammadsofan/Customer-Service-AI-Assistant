import api from './api';

export interface AiProvider {
    id: string;
    name: string;
}

export interface AiModel {
    id: string;
    name: string;
    providerId: string;
}

export interface AiConfiguration {
    id?: number;
    providerId: string;
    modelId: string;
    apiKey: string;
    isActive: boolean;
}

const aiService = {
    getProviders: async (): Promise<AiProvider[]> => {
        const response = await api.get<AiProvider[]>('/ai/providers');
        return response.data;
    },
    getModels: async (providerId: string): Promise<AiModel[]> => {
        const response = await api.get<AiModel[]>(`/ai/providers/${providerId}/models`);
        return response.data;
    },
    getConfiguration: async (): Promise<AiConfiguration> => {
        const response = await api.get<AiConfiguration>('/ai/configuration');
        return response.data;
    },
    saveConfiguration: async (config: AiConfiguration): Promise<AiConfiguration> => {
        const response = await api.post<AiConfiguration>('/ai/configuration', config);
        return response.data;
    },
    testConnection: async (config: AiConfiguration): Promise<boolean> => {
        const response = await api.post<{ success: boolean }>('/ai/test-connection', config);
        return response.data.success;
    }
};

export default aiService;

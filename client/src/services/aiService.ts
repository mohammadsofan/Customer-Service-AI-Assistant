import api from './api';

export interface AiProvider {
    id: string;
    name: string;
    providerType?: string;
    isActive?: boolean;
    fallbackPriority?: number;
    models?: AiModel[];
}

export interface AiModel {
    id: string;
    name?: string;
    modelName?: string;
    providerId: string;
    isActive?: boolean;
}

export interface AiConfiguration {
    id?: string | number;
    providerId?: string;
    activeProviderId?: string;
    modelId?: string;
    activeModelId?: string;
    apiKey?: string;
    isActive?: boolean;
    enableAutoFailover?: boolean;
    temperature?: number;
    maxTokens?: number;
    similarityThreshold?: number;
    topK?: number;
    systemPrompt?: string;
}

const aiService = {
    getProviders: async (): Promise<AiProvider[]> => {
        const response = await api.get<any>('/ai/providers');
        const data = response.data;
        return Array.isArray(data) ? data : (data?.items || []);
    },
    getModels: async (providerId?: string): Promise<AiModel[]> => {
        const url = providerId ? `/ai/models?providerId=${providerId}` : '/ai/models';
        const response = await api.get<any>(url);
        const data = response.data;
        const list = Array.isArray(data) ? data : (data?.items || []);
        return list.map((m: any) => ({
            ...m,
            name: m.name || m.modelName || 'Model'
        }));
    },
    getConfiguration: async (): Promise<AiConfiguration> => {
        const response = await api.get<AiConfiguration>('/ai/configuration');
        return response.data || {
            temperature: 0.7,
            maxTokens: 1024,
            similarityThreshold: 0.7,
            topK: 5,
            systemPrompt: 'أنت مساعد ذكي لدعم الموظفين. قدم إجابات دقيقة ومهنية باللغة العربية بناءً على قاعدة المعرفة المتاحة.',
            enableAutoFailover: true
        };
    },
    saveConfiguration: async (config: any): Promise<AiConfiguration> => {
        const payload = {
            activeProviderId: config.activeProviderId || config.providerId,
            activeModelId: config.activeModelId || config.modelId,
            temperature: config.temperature ?? 0.7,
            maxTokens: config.maxTokens ?? 1024,
            similarityThreshold: config.similarityThreshold ?? 0.7,
            topK: config.topK ?? 5,
            systemPrompt: config.systemPrompt || '',
            enableAutoFailover: config.enableAutoFailover ?? true
        };
        const response = await api.post<AiConfiguration>('/ai/configuration', payload);
        return response.data;
    },
    testConnection: async (config?: any): Promise<boolean> => {
        try {
            const providerId = config?.activeProviderId || config?.providerId;
            if (providerId) {
                await api.post(`/ai/providers/${providerId}/test`);
            }
            return true;
        } catch {
            return false;
        }
    }
};

export default aiService;

import api from './api';

export interface AiProvider {
    id: string;
    name: string;
    providerType?: string;
    baseUrl?: string;
    isActive?: boolean;
    fallbackPriority?: number;
    maskedApiKey?: string;
    modelCount?: number;
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
    activeEmbeddingProviderId?: string;
    activeEmbeddingModelId?: string;
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

    createProvider: async (provider: { name: string; providerType: string; apiKey: string; baseUrl?: string; fallbackPriority?: number }): Promise<AiProvider> => {
        const response = await api.post<AiProvider>('/ai/providers', provider);
        return response.data;
    },

    updateProvider: async (id: string, provider: { name: string; apiKey?: string; baseUrl?: string; fallbackPriority?: number }): Promise<AiProvider> => {
        const response = await api.put<AiProvider>(`/ai/providers/${id}`, provider);
        return response.data;
    },

    deleteProvider: async (id: string): Promise<void> => {
        await api.delete(`/ai/providers/${id}`);
    },

    activateProvider: async (id: string): Promise<void> => {
        await api.patch(`/ai/providers/${id}/activate`);
    },

    deactivateProvider: async (id: string): Promise<void> => {
        await api.patch(`/ai/providers/${id}/deactivate`);
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

    createModel: async (model: { providerId: string; modelName: string }): Promise<AiModel> => {
        const response = await api.post<AiModel>('/ai/models', model);
        return response.data;
    },

    updateModel: async (id: string, modelName: string): Promise<AiModel> => {
        const response = await api.put<AiModel>(`/ai/models/${id}`, { modelName });
        return response.data;
    },

    deleteModel: async (id: string): Promise<void> => {
        await api.delete(`/ai/models/${id}`);
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
            activeEmbeddingProviderId: config.activeEmbeddingProviderId,
            activeEmbeddingModelId: config.activeEmbeddingModelId,
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

    testConnection: async (config?: any): Promise<{ success: boolean; message: string; latencyMs?: number }> => {
        try {
            const providerId = config?.activeProviderId || config?.providerId || config?.id;
            if (providerId) {
                const res = await api.post<any>(`/ai/providers/${providerId}/test`);
                const isSuccess = res.data?.success === true;
                return {
                    success: isSuccess,
                    message: res.data?.message || (isSuccess ? 'تم الاتصال بنجاح بالمزود' : 'فشل الاتصال بالمزود'),
                    latencyMs: res.data?.latencyMs
                };
            }
            return { success: false, message: 'يرجى تحديد مزود لاختبار الاتصال' };
        } catch (err: any) {
            const msg = err.response?.data?.message || err.response?.data?.title || err.message || 'فشل الاتصال بالمزود';
            return { success: false, message: msg };
        }
    }
};

export default aiService;

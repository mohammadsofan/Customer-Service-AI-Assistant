import api from './api';

export interface Category {
    id: string;
    name: string;
    description?: string;
    isActive?: boolean;
    scenarioCount?: number;
}

export interface Keyword {
    id: string;
    name?: string;
    word?: string;
    scenarioCount?: number;
}

export interface Scenario {
    id: string;
    name: string;
    title?: string;
    description?: string;
    content?: string;
    categoryId: string;
    categoryName?: string;
    status?: string;
    keywords?: string[];
    steps?: string[];
    resolutionSteps?: Array<{ id: string; stepOrder: number; stepText: string }>;
    createdAt?: string;
}

const knowledgeService = {
    // Scenarios
    getScenarios: async (): Promise<Scenario[]> => {
        const response = await api.get<any>('/knowledge/scenarios');
        const data = response.data;
        return Array.isArray(data) ? data : (data?.items || []);
    },
    getScenario: async (id: string | number): Promise<Scenario> => {
        const response = await api.get<Scenario>(`/knowledge/scenarios/${id}`);
        return response.data;
    },
    createScenario: async (scenario: any): Promise<Scenario> => {
        const response = await api.post<Scenario>('/knowledge/scenarios', scenario);
        return response.data;
    },
    updateScenario: async (id: string | number, scenario: any): Promise<Scenario> => {
        const response = await api.put<Scenario>(`/knowledge/scenarios/${id}`, scenario);
        return response.data;
    },
    deleteScenario: async (id: string | number): Promise<void> => {
        await api.delete(`/knowledge/scenarios/${id}`);
    },

    // Categories
    getCategories: async (): Promise<Category[]> => {
        const response = await api.get<any>('/knowledge/categories');
        const data = response.data;
        return Array.isArray(data) ? data : (data?.items || []);
    },
    createCategory: async (category: Partial<Category>): Promise<Category> => {
        const response = await api.post<Category>('/knowledge/categories', category);
        return response.data;
    },
    updateCategory: async (id: string | number, category: Partial<Category>): Promise<Category> => {
        const response = await api.put<Category>(`/knowledge/categories/${id}`, category);
        return response.data;
    },
    deleteCategory: async (id: string | number): Promise<void> => {
        await api.delete(`/knowledge/categories/${id}`);
    },

    // Keywords
    getKeywords: async (): Promise<Keyword[]> => {
        const response = await api.get<any>('/knowledge/keywords');
        const data = response.data;
        const list = Array.isArray(data) ? data : (data?.items || []);
        return list.map((k: any) => ({
            ...k,
            word: k.word || k.name
        }));
    },
    createKeyword: async (keyword: Partial<Keyword>): Promise<Keyword> => {
        const payload = {
            name: keyword.name || keyword.word,
            word: keyword.word || keyword.name
        };
        const response = await api.post<Keyword>('/knowledge/keywords', payload);
        return response.data;
    },
    deleteKeyword: async (id: string | number): Promise<void> => {
        await api.delete(`/knowledge/keywords/${id}`);
    }
};

export default knowledgeService;

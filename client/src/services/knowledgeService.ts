import api from './api';

export interface Category {
    id: number;
    name: string;
}

export interface Keyword {
    id: number;
    word: string;
}

export interface Scenario {
    id: number;
    title: string;
    content: string;
    categoryId: number;
    keywords: string[];
}

const knowledgeService = {
    // Scenarios
    getScenarios: async (): Promise<Scenario[]> => {
        const response = await api.get<Scenario[]>('/knowledge/scenarios');
        return response.data;
    },
    getScenario: async (id: number): Promise<Scenario> => {
        const response = await api.get<Scenario>(`/knowledge/scenarios/${id}`);
        return response.data;
    },
    createScenario: async (scenario: Partial<Scenario>): Promise<Scenario> => {
        const response = await api.post<Scenario>('/knowledge/scenarios', scenario);
        return response.data;
    },
    updateScenario: async (id: number, scenario: Partial<Scenario>): Promise<Scenario> => {
        const response = await api.put<Scenario>(`/knowledge/scenarios/${id}`, scenario);
        return response.data;
    },
    deleteScenario: async (id: number): Promise<void> => {
        await api.delete(`/knowledge/scenarios/${id}`);
    },

    // Categories
    getCategories: async (): Promise<Category[]> => {
        const response = await api.get<Category[]>('/knowledge/categories');
        return response.data;
    },
    createCategory: async (category: Partial<Category>): Promise<Category> => {
        const response = await api.post<Category>('/knowledge/categories', category);
        return response.data;
    },
    updateCategory: async (id: number, category: Partial<Category>): Promise<Category> => {
        const response = await api.put<Category>(`/knowledge/categories/${id}`, category);
        return response.data;
    },
    deleteCategory: async (id: number): Promise<void> => {
        await api.delete(`/knowledge/categories/${id}`);
    },

    // Keywords
    getKeywords: async (): Promise<Keyword[]> => {
        const response = await api.get<Keyword[]>('/knowledge/keywords');
        return response.data;
    },
    createKeyword: async (keyword: Partial<Keyword>): Promise<Keyword> => {
        const response = await api.post<Keyword>('/knowledge/keywords', keyword);
        return response.data;
    },
    deleteKeyword: async (id: number): Promise<void> => {
        await api.delete(`/knowledge/keywords/${id}`);
    }
};

export default knowledgeService;

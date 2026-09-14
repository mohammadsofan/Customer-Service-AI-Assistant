import api from './api';

export interface PaginatedKnowledgeResponse<T> {
    items: T[];
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
}

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

export interface ResolutionStep {
    id?: string;
    stepOrder: number;
    stepText: string;
    description?: string;
}

export interface ScenarioStepInput {
    stepText: string;
    description?: string;
}

export interface Scenario {
    id: string;
    name: string;
    title?: string;
    description?: string;
    content?: string;
    categoryId?: string;
    categoryName?: string;
    status: string;
    keywords?: string[];
    steps?: string[];
    resolutionSteps?: ResolutionStep[];
    stepCount?: number;
    keywordCount?: number;
    createdAt?: string;
    updatedAt?: string;
}

export interface CreateScenarioDto {
    name: string;
    description: string;
    categoryId: string;
    keywords: string[];
    resolutionSteps: string[];
    steps?: ScenarioStepInput[];
    status?: string;
}

export interface UpdateScenarioDto {
    name: string;
    description: string;
    categoryId: string;
    keywords: string[];
    resolutionSteps: string[];
    steps?: ScenarioStepInput[];
}

const knowledgeService = {
    // Scenarios
    getScenarios: async (): Promise<Scenario[]> => {
        const response = await api.get<any>('/knowledge/scenarios?pageSize=100');
        const data = response.data;
        return Array.isArray(data) ? data : (data?.items || []);
    },
    getScenario: async (id: string | number): Promise<Scenario> => {
        const response = await api.get<Scenario>(`/knowledge/scenarios/${id}`);
        return response.data;
    },
    createScenario: async (scenario: CreateScenarioDto): Promise<Scenario> => {
        const response = await api.post<Scenario>('/knowledge/scenarios', scenario);
        return response.data;
    },
    updateScenario: async (id: string | number, scenario: UpdateScenarioDto): Promise<Scenario> => {
        const response = await api.put<Scenario>(`/knowledge/scenarios/${id}`, scenario);
        return response.data;
    },
    updateStatus: async (id: string | number, status: string): Promise<void> => {
        await api.patch(`/knowledge/scenarios/${id}/status`, JSON.stringify(status), {
            headers: { 'Content-Type': 'application/json' }
        });
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
    getPagedCategories: async (page = 1, pageSize = 10, search?: string): Promise<PaginatedKnowledgeResponse<Category>> => {
        const params: Record<string, any> = { page, pageSize };
        if (search && search.trim()) {
            params.search = search.trim();
        }
        const response = await api.get<any>('/knowledge/categories', { params });
        const data = response.data;
        const list = Array.isArray(data) ? data : (data?.items || []);
        const totalCount = typeof data?.totalCount === 'number' ? data.totalCount : list.length;
        const totalPages = typeof data?.totalPages === 'number'
            ? data.totalPages
            : Math.max(1, Math.ceil(totalCount / pageSize));

        return {
            items: list,
            totalCount,
            page: data?.page || page,
            pageSize: data?.pageSize || pageSize,
            totalPages
        };
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
    getPagedKeywords: async (page = 1, pageSize = 10, search?: string): Promise<PaginatedKnowledgeResponse<Keyword>> => {
        const params: Record<string, any> = { page, pageSize };
        if (search && search.trim()) {
            params.search = search.trim();
        }
        const response = await api.get<any>('/knowledge/keywords', { params });
        const data = response.data;
        const list = Array.isArray(data) ? data : (data?.items || []);
        const totalCount = typeof data?.totalCount === 'number' ? data.totalCount : list.length;
        const totalPages = typeof data?.totalPages === 'number'
            ? data.totalPages
            : Math.max(1, Math.ceil(totalCount / pageSize));

        return {
            items: list.map((k: any) => ({
                ...k,
                word: k.word || k.name
            })),
            totalCount,
            page: data?.page || page,
            pageSize: data?.pageSize || pageSize,
            totalPages
        };
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

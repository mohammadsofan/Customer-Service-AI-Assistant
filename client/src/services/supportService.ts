import api from './api';

export interface SubmitQuestionRequest {
    problem: string;
}

export interface QuestionResponse {
    id: string;
    status: string; // 'New', 'Processing', 'Answered', 'NoAnswer', 'Failed', 'Closed'
    answered: boolean;
    answer?: string;
    steps?: string[];
    confidenceScore?: number;
    sourceScenario?: string;
    escalated: boolean;
}

export interface PaginatedRequest {
    page: number;
    pageSize: number;
}

export interface QuestionHistoryDto {
    id: string;
    questionText: string;
    status: string;
    answeredByAI: boolean;
    confidenceScore?: number;
    createdAt: string;
    completedAt?: string;
}

export interface PaginatedResponse<T> {
    items: T[];
    totalCount: number;
    page: number;
    pageSize: number;
}

const supportService = {
    submitQuestion: async (request: SubmitQuestionRequest): Promise<QuestionResponse> => {
        const response = await api.post<QuestionResponse>('/support/questions', request);
        return response.data;
    },
    getQuestionById: async (id: string): Promise<QuestionResponse> => {
        const response = await api.get<QuestionResponse>(`/support/questions/${id}`);
        return response.data;
    },
    getQuestionHistory: async (params: PaginatedRequest): Promise<PaginatedResponse<QuestionHistoryDto>> => {
        const response = await api.get<PaginatedResponse<QuestionHistoryDto>>('/support/questions/history', { params });
        return response.data;
    }
};

export default supportService;

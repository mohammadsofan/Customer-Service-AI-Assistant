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
    },
    getAllQuestions: async (params: PaginatedRequest & { status?: string, date?: string }): Promise<PaginatedResponse<QuestionHistoryDto>> => {
        const queryParams: Record<string, any> = {
            page: params.page,
            pageSize: params.pageSize
        };
        if (params.status && params.status.trim() !== '') {
            queryParams.status = params.status;
        }
        if (params.date && params.date.trim() !== '') {
            queryParams.date = params.date;
        }
        const response = await api.get<PaginatedResponse<QuestionHistoryDto>>('/admin/questions', { params: queryParams });
        return response.data;
    }
};

export default supportService;

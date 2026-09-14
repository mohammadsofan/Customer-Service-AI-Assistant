import api from './api';

export interface SubmitQuestionRequest {
    problem: string;
}

export interface DetailedStepDto {
    order: number;
    text: string;
    description?: string;
}

export interface QuestionResponse {
    id: string;
    status: string; // 'New', 'Processing', 'Answered', 'NoAnswer', 'Failed', 'Closed'
    answered: boolean;
    answer?: string;
    steps?: string[];
    detailedSteps?: DetailedStepDto[];
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
    employeeId?: string;
    employeeName?: string;
    employeeEmail?: string;
    questionText: string;
    status: string;
    answeredByAI: boolean;
    confidenceScore?: number;
    createdAt: string;
    completedAt?: string;
}

export interface TopScenarioDto {
    id: string;
    name: string;
    description: string;
    usageCount: number;
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
    },
    getTopScenarios: async (count: number = 5): Promise<TopScenarioDto[]> => {
        const response = await api.get<TopScenarioDto[]>('/support/questions/top-scenarios', { params: { count } });
        return response.data;
    }
};

export default supportService;

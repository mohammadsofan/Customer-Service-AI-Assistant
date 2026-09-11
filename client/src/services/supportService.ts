import api from './api';

export interface SubmitQuestionRequest {
    question: string;
    employeeId?: string;
}

export interface SubmitQuestionResponse {
    answer: string;
    scenarioId?: number;
    confidenceScore: number;
}

export interface QuestionHistoryItem {
    id: number;
    question: string;
    answer: string;
    timestamp: string;
    isAnswered: boolean;
    employeeId?: string;
}

const supportService = {
    submitQuestion: async (request: SubmitQuestionRequest): Promise<SubmitQuestionResponse> => {
        const response = await api.post<SubmitQuestionResponse>('/support/ask', request);
        return response.data;
    },
    getQuestionHistory: async (): Promise<QuestionHistoryItem[]> => {
        const response = await api.get<QuestionHistoryItem[]>('/support/history');
        return response.data;
    }
};

export default supportService;

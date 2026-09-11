import api from './api';

export interface AnalyticsOverview {
    totalQuestions: number;
    answeredQuestions: number;
    unansweredQuestions: number;
    successRate: number;
    questionsToday?: number;
    escalated?: number;
    avgResponseTime?: string;
}

export interface QuestionAnalytics {
    date: string;
    count: number;
}

export interface KnowledgeAnalytics {
    categoryId: number;
    categoryName: string;
    usageCount: number;
}

export interface UnansweredQuestion {
    id: number;
    question: string;
    timestamp: string;
    employeeId?: string;
}

const analyticsService = {
    getOverview: async (): Promise<AnalyticsOverview> => {
        const response = await api.get<AnalyticsOverview>('/analytics/overview');
        return response.data;
    },
    getQuestionAnalytics: async (startDate: string, endDate: string): Promise<QuestionAnalytics[]> => {
        const response = await api.get<QuestionAnalytics[]>('/analytics/questions', { params: { startDate, endDate } });
        return response.data;
    },
    getKnowledgeAnalytics: async (): Promise<KnowledgeAnalytics[]> => {
        const response = await api.get<KnowledgeAnalytics[]>('/analytics/knowledge');
        return response.data;
    },
    getUnanswered: async (): Promise<UnansweredQuestion[]> => {
        const response = await api.get<UnansweredQuestion[]>('/analytics/unanswered');
        return response.data;
    }
};

export default analyticsService;

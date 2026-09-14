import api from './api';

export interface AnalyticsOverview {
    totalQuestions: number;
    todayQuestions?: number;
    questionsToday?: number;
    answeredCount?: number;
    answeredQuestions?: number;
    noAnswerCount?: number;
    unansweredQuestions?: number;
    escalatedCount?: number;
    escalated?: number;
    avgResponseTimeMs?: number;
    avgResponseTime?: string;
    avgSimilarityScore?: number;
    answerRate?: number;
    successRate?: number;
}

export interface QuestionAnalytics {
    id?: string;
    date?: string;
    count?: number;
    questionText?: string;
    status?: string;
    answeredByAI?: boolean;
    confidenceScore?: number;
    processingTimeMs?: number;
    scenarioName?: string;
    createdAt?: string;
}

export interface KnowledgeAnalytics {
    id?: string | number;
    scenarioId?: string;
    categoryId?: string | number;
    categoryName?: string;
    scenarioName?: string;
    usageCount?: number;
    retrievalCount?: number;
    avgSimilarityScore?: number;
}

export interface CategoryAnalytics {
    categoryId: string;
    categoryName: string;
    scenarioCount: number;
    questionCount: number;
    percentage: number;
}

export interface PaginatedResult<T> {
    items: T[];
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
}

export interface UnansweredQuestion {
    id: string | number;
    question?: string;
    questionText?: string;
    timestamp?: string;
    createdAt?: string;
    firstAsked?: string;
    frequency?: number;
    employeeId?: string;
    employeeName?: string;
    employeeEmail?: string;
}

const analyticsService = {
    getOverview: async (startDate?: string, endDate?: string): Promise<AnalyticsOverview> => {
        const response = await api.get<any>('/analytics/overview', { params: { fromDate: startDate, toDate: endDate } });
        const raw = response.data || {};
        
        const total = raw.totalQuestions ?? 0;
        const answered = raw.answeredCount ?? raw.answeredQuestions ?? 0;
        const escalated = raw.escalatedCount ?? raw.escalated ?? 0;
        const unanswered = raw.noAnswerCount ?? raw.unansweredQuestions ?? 0;
        const today = raw.todayQuestions ?? raw.questionsToday ?? 0;
        const rate = raw.answerRate ?? raw.successRate ?? (total > 0 ? (answered / total) : 0);
        const avgMs = raw.avgResponseTimeMs ?? 0;
        const avgTimeFormatted = avgMs > 0 ? `${(avgMs / 1000).toFixed(2)}s` : '0.4s';

        return {
            totalQuestions: total,
            questionsToday: today,
            todayQuestions: today,
            answeredQuestions: answered,
            answeredCount: answered,
            unansweredQuestions: unanswered,
            noAnswerCount: unanswered,
            escalated: escalated,
            escalatedCount: escalated,
            successRate: rate,
            answerRate: rate,
            avgResponseTime: avgTimeFormatted,
            avgResponseTimeMs: avgMs,
            avgSimilarityScore: raw.avgSimilarityScore ?? 0
        };
    },
    getQuestionAnalytics: async (startDate?: string, endDate?: string): Promise<QuestionAnalytics[]> => {
        const response = await api.get<any>('/analytics/questions', { params: { fromDate: startDate, toDate: endDate } });
        const data = response.data;
        const list = data?.questions || (Array.isArray(data) ? data : []);
        return list;
    },
    getKnowledgeAnalytics: async (page = 1, pageSize = 8, startDate?: string, endDate?: string): Promise<PaginatedResult<KnowledgeAnalytics>> => {
        const response = await api.get<any>('/analytics/knowledge', { params: { page, pageSize, fromDate: startDate, toDate: endDate } });
        const data = response.data;
        const list = Array.isArray(data) ? data : (data?.items || []);
        const totalCount = data?.totalCount ?? list.length;
        const totalPages = data?.totalPages ?? Math.max(1, Math.ceil(totalCount / pageSize));
        const items = list.map((item: any) => ({
            id: item.scenarioId || item.categoryId || item.id,
            scenarioId: item.scenarioId,
            scenarioName: item.scenarioName || 'سيناريو عام',
            categoryName: item.categoryName || 'غير مصنف',
            usageCount: item.retrievalCount ?? item.usageCount ?? 0,
            avgSimilarityScore: item.avgSimilarityScore ?? 0
        }));
        return { items, totalCount, page, pageSize, totalPages };
    },
    getCategoryAnalytics: async (page = 1, pageSize = 5, startDate?: string, endDate?: string): Promise<PaginatedResult<CategoryAnalytics>> => {
        const response = await api.get<any>('/analytics/categories', { params: { page, pageSize, fromDate: startDate, toDate: endDate } });
        const data = response.data;
        const list = Array.isArray(data) ? data : (data?.items || []);
        const totalCount = data?.totalCount ?? list.length;
        const totalPages = data?.totalPages ?? Math.max(1, Math.ceil(totalCount / pageSize));
        return { items: list, totalCount, page, pageSize, totalPages };
    },
    getUnanswered: async (page = 1, pageSize = 8, sortOrder = 'desc', startDate?: string, endDate?: string): Promise<PaginatedResult<UnansweredQuestion>> => {
        const response = await api.get<any>('/analytics/unanswered', { params: { page, pageSize, sortOrder, fromDate: startDate, toDate: endDate } });
        const data = response.data;
        const list = data?.questions || (Array.isArray(data) ? data : (data?.items || []));
        const totalCount = data?.totalCount ?? list.length;
        const totalPages = data?.totalPages ?? Math.max(1, Math.ceil(totalCount / pageSize));
        const items = list.map((u: any) => ({
            id: u.id,
            question: u.questionText || u.question || '',
            questionText: u.questionText || u.question || '',
            timestamp: u.lastAsked || u.firstAsked || u.createdAt || new Date().toISOString(),
            frequency: u.frequency || 1,
            employeeId: u.employeeId,
            employeeName: u.employeeName,
            employeeEmail: u.employeeEmail
        }));
        return { items, totalCount, page, pageSize, totalPages };
    }
};

export default analyticsService;

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

export interface UnansweredQuestion {
    id: string | number;
    question?: string;
    questionText?: string;
    timestamp?: string;
    createdAt?: string;
    firstAsked?: string;
    frequency?: number;
    employeeId?: string;
}

const analyticsService = {
    getOverview: async (): Promise<AnalyticsOverview> => {
        const response = await api.get<any>('/analytics/overview');
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
    getKnowledgeAnalytics: async (): Promise<KnowledgeAnalytics[]> => {
        const response = await api.get<any>('/analytics/knowledge');
        const data = response.data;
        const list = Array.isArray(data) ? data : (data?.items || []);
        return list.map((item: any) => ({
            id: item.scenarioId || item.categoryId || item.id,
            scenarioId: item.scenarioId,
            scenarioName: item.scenarioName || item.categoryName || 'سيناريو عام',
            categoryName: item.scenarioName || item.categoryName || 'سيناريو عام',
            usageCount: item.retrievalCount ?? item.usageCount ?? 0,
            avgSimilarityScore: item.avgSimilarityScore ?? 0
        }));
    },
    getUnanswered: async (): Promise<UnansweredQuestion[]> => {
        const response = await api.get<any>('/analytics/unanswered');
        const data = response.data;
        const list = data?.questions || (Array.isArray(data) ? data : []);
        return list.map((u: any) => ({
            id: u.id,
            question: u.questionText || u.question || '',
            questionText: u.questionText || u.question || '',
            timestamp: u.lastAsked || u.firstAsked || u.createdAt || new Date().toISOString(),
            frequency: u.frequency || 1
        }));
    }
};

export default analyticsService;

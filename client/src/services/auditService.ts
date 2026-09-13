import api from './api';

export interface AuditLog {
    id: string | number;
    action: string;
    details?: string;
    entityType?: string;
    metadata?: string;
    userId?: string;
    userName: string;
    userEmail?: string;
    userRole?: string;
    timestamp: string;
}

export interface PaginatedAuditLogs {
    items: AuditLog[];
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
}

const auditService = {
    getAuditLogs: async (page = 1, pageSize = 15, search?: string): Promise<PaginatedAuditLogs> => {
        const params: Record<string, any> = { page, pageSize };
        if (search && search.trim()) {
            params.search = search.trim();
        }
        const response = await api.get<any>('/audit/logs', { params });
        const data = response.data;
        const rawList = Array.isArray(data) ? data : (data?.items || []);
        const totalCount = typeof data?.totalCount === 'number' ? data.totalCount : rawList.length;
        const totalPages = typeof data?.totalPages === 'number' 
            ? data.totalPages 
            : Math.max(1, Math.ceil(totalCount / pageSize));

        const items = rawList.map((log: any) => ({
            id: log.id,
            action: log.action || 'عملية في النظام',
            details: log.metadata || log.details || (log.entityType ? `${log.entityType}: ${log.action}` : 'عملية في النظام'),
            userId: log.userId,
            userName: log.userName || (log.userId === '00000000-0000-0000-0000-000000000000' || !log.userId ? 'النظام' : 'مستخدم غير معروف'),
            userEmail: log.userEmail,
            userRole: log.userRole,
            timestamp: log.timestamp || log.createdAt || new Date().toISOString()
        }));

        return {
            items,
            totalCount,
            page: data?.page || page,
            pageSize: data?.pageSize || pageSize,
            totalPages
        };
    }
};

export default auditService;

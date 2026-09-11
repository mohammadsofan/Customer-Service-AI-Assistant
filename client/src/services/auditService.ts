import api from './api';

export interface AuditLog {
    id: string | number;
    action: string;
    details?: string;
    entityType?: string;
    metadata?: string;
    userId: string;
    timestamp: string;
}

const auditService = {
    getAuditLogs: async (): Promise<AuditLog[]> => {
        const response = await api.get<any>('/audit/logs');
        const data = response.data;
        const list = Array.isArray(data) ? data : (data?.items || []);
        return list.map((log: any) => ({
            id: log.id,
            action: log.action || 'عملية في النظام',
            details: log.metadata || log.details || `${log.entityType || 'عنصر'}: ${log.action}`,
            userId: log.userId || 'النظام',
            timestamp: log.timestamp || log.createdAt || new Date().toISOString()
        }));
    }
};

export default auditService;

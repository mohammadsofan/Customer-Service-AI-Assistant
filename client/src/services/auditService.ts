import api from './api';

export interface AuditLog {
    id: number;
    action: string;
    details: string;
    userId: string;
    timestamp: string;
}

const auditService = {
    getAuditLogs: async (): Promise<AuditLog[]> => {
        const response = await api.get<AuditLog[]>('/audit/logs');
        return response.data;
    }
};

export default auditService;

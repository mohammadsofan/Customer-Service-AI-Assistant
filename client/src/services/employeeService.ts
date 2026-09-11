import api from './api';
import { type User } from './authService';

export interface Employee extends User {
    department?: string;
}

const employeeService = {
    getEmployees: async (): Promise<Employee[]> => {
        const response = await api.get<any>('/employees');
        const data = response.data;
        const list = Array.isArray(data) ? data : (data?.items || []);
        return list.map((emp: any) => ({
            ...emp,
            username: emp.fullName || emp.username || emp.email,
            fullName: emp.fullName || emp.username || emp.email,
            department: emp.department || 'خدمة العملاء'
        }));
    },
    getEmployee: async (id: string): Promise<Employee> => {
        const response = await api.get<Employee>(`/employees/${id}`);
        return response.data;
    },
    createEmployee: async (employee: Partial<Employee>): Promise<Employee> => {
        const payload = {
            fullName: employee.fullName || employee.username,
            email: employee.email,
            password: (employee as any).password || 'Employee123!',
            role: employee.role || 'Employee'
        };
        const response = await api.post<Employee>('/employees', payload);
        return response.data;
    },
    updateEmployee: async (id: string, employee: Partial<Employee>): Promise<Employee> => {
        const payload = {
            fullName: employee.fullName || employee.username,
            email: employee.email,
            role: employee.role || 'Employee'
        };
        const response = await api.put<Employee>(`/employees/${id}`, payload);
        return response.data;
    },
    deleteEmployee: async (id: string): Promise<void> => {
        await api.patch(`/employees/${id}/deactivate`);
    }
};

export default employeeService;

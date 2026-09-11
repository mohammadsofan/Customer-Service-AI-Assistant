import api from './api';
import { type User } from './authService';

export interface Employee extends User {
    department?: string;
}

const employeeService = {
    getEmployees: async (): Promise<Employee[]> => {
        const response = await api.get<Employee[]>('/employees');
        return response.data;
    },
    getEmployee: async (id: string): Promise<Employee> => {
        const response = await api.get<Employee>(`/employees/${id}`);
        return response.data;
    },
    createEmployee: async (employee: Partial<Employee>): Promise<Employee> => {
        const response = await api.post<Employee>('/employees', employee);
        return response.data;
    },
    updateEmployee: async (id: string, employee: Partial<Employee>): Promise<Employee> => {
        const response = await api.put<Employee>(`/employees/${id}`, employee);
        return response.data;
    },
    deleteEmployee: async (id: string): Promise<void> => {
        await api.delete(`/employees/${id}`);
    }
};

export default employeeService;

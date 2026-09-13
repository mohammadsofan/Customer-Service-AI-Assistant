import React, { useState, useEffect } from 'react';
import { Button } from '../components/Button';
import { DataTable, type Column } from '../components/DataTable';
import { SearchBox } from '../components/SearchBox';
import { Modal } from '../components/Modal';
import { Input } from '../components/Input';
import { Select } from '../components/Select';
import { StatusBadge } from '../components/StatusBadge';
import { toast } from 'react-hot-toast';
import employeeService, { type Employee } from '../services/employeeService';

export function EmployeesPage() {
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [currentEmployee, setCurrentEmployee] = useState<Employee | null>(null);

  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [role, setRole] = useState('agent');
  const [department, setDepartment] = useState('');
  const [isActive, setIsActive] = useState(true);

  useEffect(() => {
    const timer = setTimeout(() => {
      loadEmployees(page, search);
    }, 300);
    return () => clearTimeout(timer);
  }, [page, search]);

  const loadEmployees = async (currentPage = page, currentSearch = search) => {
    try {
      setIsLoading(true);
      const data = await employeeService.getEmployees(currentPage, 10, currentSearch);
      setEmployees(data.items.map(e => ({ ...e, isActive: (e as any).isActive ?? true })));
      setTotalPages(data.totalPages);
      setTotalCount(data.totalCount);
    } catch (error) {
      toast.error('حدث خطأ أثناء تحميل الموظفين');
    } finally {
      setIsLoading(false);
    }
  };

  const handleOpenModal = (employee?: Employee) => {
    if (employee) {
      setCurrentEmployee(employee);
      setUsername(employee.username || '');
      setEmail(employee.email);
      setRole(employee.role);
      setDepartment(employee.department || '');
      setIsActive((employee as any).isActive ?? true);
    } else {
      setCurrentEmployee(null);
      setUsername('');
      setEmail('');
      setRole('agent');
      setDepartment('');
      setIsActive(true);
    }
    setIsModalOpen(true);
  };

  const handleSave = async () => {
    if (!username || !email) {
      toast.error('يرجى تعبئة الحقول المطلوبة');
      return;
    }
    
    try {
      const payload: Partial<Employee> = { username, email, role, department, isAdmin: role === 'admin' };
      (payload as any).isActive = isActive;
      
      if (currentEmployee) {
        const updated = await employeeService.updateEmployee(currentEmployee.id, payload);
        setEmployees(employees.map(e => e.id === currentEmployee.id ? { ...updated, isActive } : e));
        toast.success('تم التحديث بنجاح');
      } else {
        const created = await employeeService.createEmployee(payload);
        setEmployees([...employees, { ...created, isActive } as Employee]);
        toast.success('تمت الإضافة بنجاح');
      }
      setIsModalOpen(false);
    } catch {
      toast.error('حدث خطأ أثناء الحفظ');
    }
  };

  const handleToggleActive = async (employee: Employee) => {
    try {
      const newStatus = !(employee as any).isActive;
      await employeeService.updateEmployee(employee.id, { ...employee, isActive: newStatus } as any);
      setEmployees(employees.map(e => e.id === employee.id ? { ...e, isActive: newStatus } : e));
      toast.success('تم تغيير حالة الموظف بنجاح');
    } catch {
      toast.error('حدث خطأ أثناء تغيير الحالة');
    }
  };

  const handleDelete = async (id: string) => {
    if (window.confirm('هل أنت متأكد من الحذف؟')) {
      try {
        await employeeService.deleteEmployee(id);
        setEmployees(employees.filter(e => e.id !== id));
        toast.success('تم الحذف بنجاح');
      } catch {
        toast.error('حدث خطأ أثناء الحذف');
      }
    }
  };

  const columns: Column<Employee>[] = [
    { key: 'username', header: 'اسم المستخدم' },
    { key: 'email', header: 'البريد الإلكتروني' },
    { key: 'role', header: 'الصلاحية', cell: (item) => item.role === 'admin' ? 'مدير' : 'موظف دعم' },
    { key: 'department', header: 'القسم', cell: (item) => item.department || '-' },
    { 
      key: 'isActive', 
      header: 'الحالة',
      cell: (item) => <StatusBadge status={(item as any).isActive ? 'نشط' : 'غير نشط'} />
    },
    {
      key: 'actions',
      header: 'الإجراءات',
      cell: (item) => (
        <div className="flex gap-2">
          <Button variant="secondary" onClick={() => handleToggleActive(item)}>
            {(item as any).isActive ? 'إيقاف' : 'تفعيل'}
          </Button>
          <Button variant="outline" onClick={() => handleOpenModal(item)}>تعديل</Button>
          <Button variant="danger" onClick={() => handleDelete(item.id)}>حذف</Button>
        </div>
      )
    }
  ];

  return (
    <div className="p-6" dir="rtl">
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">إدارة الموظفين</h1>
        <Button onClick={() => handleOpenModal()}>إضافة موظف جديد</Button>
      </div>

      <div className="mb-4 max-w-md">
        <SearchBox
          value={search}
          onChange={(e) => {
            setSearch(e.target.value);
            setPage(1);
          }}
          placeholder="ابحث بالاسم أو البريد..."
        />
      </div>

      {isLoading ? (
        <div>جاري التحميل...</div>
      ) : (
        <DataTable
          data={employees}
          columns={columns}
          currentPage={page}
          totalPages={totalPages}
          onPageChange={setPage}
        />
      )}

      <Modal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={currentEmployee ? 'تعديل بيانات موظف' : 'إضافة موظف جديد'}
      >
        <div className="space-y-4">
          <Input
            label="اسم المستخدم"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
          />
          <Input
            label="البريد الإلكتروني"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />
          <Select
            label="الصلاحية"
            value={role}
            onChange={(e) => setRole(e.target.value)}
            options={[
              { value: 'agent', label: 'موظف دعم' },
              { value: 'admin', label: 'مدير' }
            ]}
          />
          <Input
            label="القسم (اختياري)"
            value={department}
            onChange={(e) => setDepartment(e.target.value)}
          />
          <div className="mt-6 flex justify-end gap-3">
            <Button variant="outline" onClick={() => setIsModalOpen(false)}>
              إلغاء
            </Button>
            <Button onClick={handleSave}>
              حفظ
            </Button>
          </div>
        </div>
      </Modal>
    </div>
  );
}

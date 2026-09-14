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

import { Eye, EyeOff, KeyRound, Sparkles, Copy } from 'lucide-react';

export function EmployeesPage() {
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [initialLoading, setInitialLoading] = useState(true);
  const [isSearching, setIsSearching] = useState(false);
  
  const [search, setSearch] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [currentEmployee, setCurrentEmployee] = useState<Employee | null>(null);

  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [role, setRole] = useState('agent');
  const [department, setDepartment] = useState('');
  const [isActive, setIsActive] = useState(true);

  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearch(search);
      setPage(1);
    }, 400);
    return () => clearTimeout(timer);
  }, [search]);

  useEffect(() => {
    loadEmployees(page, debouncedSearch);
  }, [page, debouncedSearch]);

  const loadEmployees = async (currentPage = page, currentSearch = debouncedSearch) => {
    try {
      setIsSearching(true);
      const data = await employeeService.getEmployees(currentPage, 10, currentSearch);
      setEmployees(data.items.map(e => ({ ...e, isActive: (e as any).isActive ?? true })));
      setTotalPages(data.totalPages);
      setTotalCount(data.totalCount);
    } catch (error) {
      toast.error('حدث خطأ أثناء تحميل الموظفين');
    } finally {
      setIsSearching(false);
      setInitialLoading(false);
    }
  };

  const generateRandomPassword = () => {
    const chars = 'ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%';
    let res = 'Emp#';
    for (let i = 0; i < 6; i++) {
      res += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    setPassword(res);
    setShowPassword(true);
    toast.success('تم توليد كلمة مرور آمنة للموظف');
  };

  const handleOpenModal = (employee?: Employee) => {
    setPassword('');
    setShowPassword(false);
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
      toast.error('يرجى تعبئة الحقول المطلوبة (الاسم والبريد)');
      return;
    }

    if (!currentEmployee && (!password || password.length < 8)) {
      toast.error('يرجى إدخال كلمة مرور للموظف الجديد (8 أحرف على الأقل)');
      return;
    }

    if (currentEmployee && password && password.length < 8) {
      toast.error('كلمة المرور الجديدة يجب أن تكون 8 أحرف على الأقل');
      return;
    }
    
    try {
      const payload: Partial<Employee> = { 
        username, 
        fullName: username,
        email, 
        role, 
        department, 
        isAdmin: role === 'admin' 
      };
      (payload as any).isActive = isActive;
      if (password && password.trim()) {
        payload.password = password.trim();
      }
      
      if (currentEmployee) {
        const updated = await employeeService.updateEmployee(currentEmployee.id, payload);
        setEmployees(employees.map(e => e.id === currentEmployee.id ? { ...updated, isActive } : e));
        toast.success('تم تحديث بيانات الموظف بنجاح');
      } else {
        const created = await employeeService.createEmployee(payload);
        setEmployees([...employees, { ...created, isActive } as Employee]);
        toast.success(`تم إنشاء الموظف بنجاح! كلمة المرور: ${password || 'Employee123!'}`);
      }
      setIsModalOpen(false);
    } catch (err: any) {
      const msg = err?.response?.data?.message || err?.response?.data?.title || 'حدث خطأ أثناء الحفظ';
      toast.error(msg);
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
          onChange={(e) => setSearch(e.target.value)}
          loading={isSearching}
          placeholder="ابحث بالاسم أو البريد..."
        />
      </div>

      {initialLoading ? (
        <div className="py-12 text-center text-gray-500">جاري التحميل...</div>
      ) : (
        <div className={isSearching ? 'opacity-70 transition-opacity' : 'transition-opacity'}>
          <DataTable
            data={employees}
            columns={columns}
            currentPage={page}
            totalPages={totalPages}
            onPageChange={setPage}
          />
        </div>
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

          <div>
            <div className="flex items-center justify-between mb-1.5">
              <label className="block text-sm font-medium text-slate-700 flex items-center gap-1.5">
                <KeyRound className="w-3.5 h-3.5 text-[#0055b8]" />
                <span>{currentEmployee ? 'تغيير كلمة المرور (اختياري)' : 'كلمة المرور'}</span>
              </label>
              <button
                type="button"
                onClick={generateRandomPassword}
                className="text-xs text-[#0055b8] hover:text-[#003882] font-semibold flex items-center gap-1 cursor-pointer bg-blue-50/70 hover:bg-blue-100/70 px-2 py-0.5 rounded-md transition-colors"
              >
                <Sparkles className="w-3 h-3 text-[#76bc21]" />
                <span>توليد كلمة مرور عشوائية</span>
              </button>
            </div>
            <div className="relative">
              <input
                type={showPassword ? 'text' : 'password'}
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder={currentEmployee ? 'اتركه فارغاً للإبقاء على كلمة المرور الحالية' : 'أدخل كلمة مرور الموظف (8 أحرف على الأقل)'}
                dir="ltr"
                className="w-full px-4 py-2.5 bg-white border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-[#76bc21]/30 focus:border-[#76bc21] text-sm text-right pr-4 pl-14 transition-all"
              />
              <button
                type="button"
                onClick={() => setShowPassword(!showPassword)}
                className="absolute left-2.5 top-1/2 -translate-y-1/2 text-xs text-slate-500 hover:text-slate-800 bg-slate-100 hover:bg-slate-200 px-2 py-1 rounded-md transition-colors cursor-pointer"
              >
                {showPassword ? 'إخفاء' : 'إظهار'}
              </button>
            </div>
            <p className="text-[11px] text-slate-400 mt-1 leading-relaxed">
              {currentEmployee 
                ? 'اترك الحقل فارغاً إذا كنت لا ترغب بتعديل كلمة المرور الحالية للموظف.' 
                : 'سيستخدم الموظف هذه الكلمة مع بريده الإلكتروني لتسجيل الدخول إلى النظام (الحد الأدنى 8 أحرف).'}
            </p>
          </div>
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

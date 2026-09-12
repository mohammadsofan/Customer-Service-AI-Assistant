import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Button } from '../components/Button';
import { DataTable } from '../components/DataTable';
import { Input } from '../components/Input';
import { Select } from '../components/Select';
import { LoadingState } from '../components/LoadingState';
import { ErrorState } from '../components/ErrorState';
import { ConfirmationDialog } from '../components/ConfirmationDialog';
import knowledgeService, { Scenario, Category } from '../services/knowledgeService';
import { Plus, Edit2, Trash2, Power } from 'lucide-react';

export function ScenarioListPage() {
  const navigate = useNavigate();
  const [scenarios, setScenarios] = useState<Scenario[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [searchTerm, setSearchTerm] = useState('');
  const [categoryFilter, setCategoryFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');

  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [deletingId, setDeletingId] = useState<string | number | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      setLoading(true);
      const [scenariosData, categoriesData] = await Promise.all([
        knowledgeService.getScenarios(),
        knowledgeService.getCategories(),
      ]);
      setScenarios(scenariosData);
      setCategories(categoriesData);
    } catch (err) {
      setError('فشل في جلب السيناريوهات.');
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteClick = (id: string | number) => {
    setDeletingId(id);
    setIsDeleteOpen(true);
  };

  const handleConfirmDelete = async () => {
    if (deletingId === null) return;
    try {
      setIsDeleting(true);
      await knowledgeService.deleteScenario(deletingId);
      await fetchData();
      setIsDeleteOpen(false);
    } catch (err) {
      console.error(err);
      alert('حدث خطأ أثناء الحذف.');
    } finally {
      setIsDeleting(false);
      setDeletingId(null);
    }
  };

  const handleToggleStatus = async (scenario: Scenario) => {
    try {
      const newStatus = scenario.status === 'Active' ? 'Inactive' : 'Active';
      await knowledgeService.updateStatus(scenario.id, newStatus);
      await fetchData();
    } catch (err: any) {
      console.error(err);
      alert('حدث خطأ أثناء تحديث الحالة.');
    }
  };

  const filteredScenarios = scenarios.filter((s) => {
    const titleOrName = (s.name || s.title || '').toLowerCase();
    const matchesSearch = titleOrName.includes(searchTerm.toLowerCase());
    const matchesCategory = categoryFilter ? (String(s.categoryId) === String(categoryFilter) || s.categoryName === categoryFilter) : true;
    const matchesStatus = statusFilter ? s.status === statusFilter : true;
    return matchesSearch && matchesCategory && matchesStatus;
  });

  const columns = [
    { 
      key: 'name', 
      header: 'اسم السيناريو',
      cell: (item: Scenario) => (
        <span className="font-medium text-gray-900">{item.name || item.title || 'بدون عنوان'}</span>
      )
    },
    { 
      key: 'categoryName', 
      header: 'التصنيف',
      cell: (item: Scenario) => item.categoryName || '-'
    },
    {
      key: 'stepCount',
      header: 'عدد الخطوات',
      cell: (item: Scenario) => item.stepCount ?? (item.resolutionSteps?.length || item.steps?.length || 0)
    },
    { 
      key: 'status', 
      header: 'الحالة',
      cell: (item: Scenario) => (
        <span className={`px-2.5 py-1 rounded-full text-xs font-medium ${item.status === 'Active' ? 'bg-green-100 text-green-800' : (item.status === 'Draft' ? 'bg-yellow-100 text-yellow-800' : 'bg-gray-100 text-gray-800')}`}>
          {item.status === 'Active' ? 'نشط' : (item.status === 'Draft' ? 'مسودة' : 'غير نشط')}
        </span>
      )
    },
    {
      key: 'actions',
      header: 'الإجراءات',
      cell: (item: Scenario) => (
        <div className="flex gap-2">
          <Button variant="outline" size="sm" onClick={() => navigate(`/admin/knowledge/edit/${item.id}`)} title="تعديل">
            <Edit2 className="w-4 h-4" />
          </Button>
          <Button variant="outline" size="sm" onClick={() => handleToggleStatus(item)} title="تبديل الحالة">
            <Power className="w-4 h-4" />
          </Button>
          <Button variant="danger" size="sm" onClick={() => handleDeleteClick(item.id)} title="حذف">
            <Trash2 className="w-4 h-4" />
          </Button>
        </div>
      ),
    },
  ];

  if (loading) return <LoadingState />;
  if (error) return <ErrorState message={error} onRetry={fetchData} />;

  return (
    <div className="space-y-6" dir="rtl">
      <div className="flex justify-between items-center">
        <h1 className="text-2xl font-bold">السيناريوهات</h1>
        <Button onClick={() => navigate('/admin/knowledge/create')}>
          <Plus className="w-4 h-4 ml-2" />
          إضافة سيناريو +
        </Button>
      </div>

      <div className="flex gap-4 mb-4">
        <div className="flex-1">
          <Input
            placeholder="بحث في السيناريوهات..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>
        <div className="w-48">
          <Select
            options={[
              { value: '', label: 'كل التصنيفات' },
              ...categories.map(c => ({ value: c.id.toString(), label: c.name }))
            ]}
            value={categoryFilter}
            onChange={(e) => setCategoryFilter(e.target.value)}
          />
        </div>
        <div className="w-48">
          <Select
            options={[
              { value: '', label: 'كل الحالات' },
              { value: 'Active', label: 'نشط' },
              { value: 'Draft', label: 'مسودة' },
              { value: 'Inactive', label: 'غير نشط' }
            ]}
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
          />
        </div>
      </div>

      <DataTable data={filteredScenarios} columns={columns} />

      <ConfirmationDialog
        isOpen={isDeleteOpen}
        onClose={() => setIsDeleteOpen(false)}
        onConfirm={handleConfirmDelete}
        title="تأكيد الحذف"
        message="هل أنت متأكد أنك تريد حذف هذا السيناريو؟"
        confirmText="حذف"
        cancelText="إلغاء"
        isLoading={isDeleting}
      />
    </div>
  );
}

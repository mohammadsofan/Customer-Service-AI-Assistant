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
import { Plus, Edit2, Trash2, Eye, Power } from 'lucide-react';

interface ExtendedScenario extends Scenario {
  status?: string;
  description?: string;
  steps?: string[];
}

export function ScenarioListPage() {
  const navigate = useNavigate();
  const [scenarios, setScenarios] = useState<ExtendedScenario[]>([]);
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

  const handleToggleStatus = async (scenario: ExtendedScenario) => {
    try {
      const newStatus = scenario.status === 'active' ? 'inactive' : 'active';
      await knowledgeService.updateScenario(scenario.id, { status: newStatus } as any);
      await fetchData();
    } catch (err) {
      console.error(err);
      alert('حدث خطأ أثناء تحديث الحالة.');
    }
  };

  const filteredScenarios = scenarios.filter((s) => {
    const titleOrName = (s.title || s.name || '').toLowerCase();
    const matchesSearch = titleOrName.includes(searchTerm.toLowerCase());
    const matchesCategory = categoryFilter ? String(s.categoryId) === String(categoryFilter) : true;
    const matchesStatus = statusFilter ? s.status === statusFilter : true;
    return matchesSearch && matchesCategory && matchesStatus;
  });

  const columns = [
    { 
      key: 'title', 
      header: 'العنوان',
      cell: (item: ExtendedScenario) => item.title || item.name || 'بدون عنوان'
    },
    { 
      key: 'categoryId', 
      header: 'التصنيف',
      cell: (item: ExtendedScenario) => {
        const cat = categories.find(c => c.id === item.categoryId);
        return cat ? cat.name : '-';
      }
    },
    { 
      key: 'status', 
      header: 'الحالة',
      cell: (item: ExtendedScenario) => (
        <span className={`px-2 py-1 rounded text-xs ${item.status === 'active' ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-800'}`}>
          {item.status === 'active' ? 'نشط' : 'غير نشط'}
        </span>
      )
    },
    {
      key: 'actions',
      header: 'الإجراءات',
      cell: (item: ExtendedScenario) => (
        <div className="flex gap-2">
          <Button variant="outline" size="sm" onClick={() => navigate(`/admin/knowledge/view/${item.id}`)}>
            <Eye className="w-4 h-4" />
          </Button>
          <Button variant="outline" size="sm" onClick={() => navigate(`/admin/knowledge/edit/${item.id}`)}>
            <Edit2 className="w-4 h-4" />
          </Button>
          <Button variant="outline" size="sm" onClick={() => handleToggleStatus(item)}>
            <Power className="w-4 h-4" />
          </Button>
          <Button variant="danger" size="sm" onClick={() => handleDeleteClick(item.id)}>
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
            placeholder="بحث..."
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
              { value: 'active', label: 'نشط' },
              { value: 'inactive', label: 'غير نشط' }
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

import { useEffect, useState } from 'react';
import { Button } from '../components/Button';
import { DataTable } from '../components/DataTable';
import { Modal } from '../components/Modal';
import { Input } from '../components/Input';
import { LoadingState } from '../components/LoadingState';
import { ErrorState } from '../components/ErrorState';
import { ConfirmationDialog } from '../components/ConfirmationDialog';
import knowledgeService, { Category } from '../services/knowledgeService';
import { Plus, Edit2, Trash2 } from 'lucide-react';

export function CategoriesPage() {
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingCategory, setEditingCategory] = useState<Category | null>(null);
  const [name, setName] = useState('');
  const [isSaving, setIsSaving] = useState(false);

  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [deletingId, setDeletingId] = useState<number | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);

  useEffect(() => {
    fetchCategories();
  }, []);

  const fetchCategories = async () => {
    try {
      setLoading(true);
      const data = await knowledgeService.getCategories();
      setCategories(data);
    } catch (err) {
      setError('فشل في جلب التصنيفات.');
    } finally {
      setLoading(false);
    }
  };

  const handleOpenModal = (category?: Category) => {
    if (category) {
      setEditingCategory(category);
      setName(category.name);
    } else {
      setEditingCategory(null);
      setName('');
    }
    setIsModalOpen(true);
  };

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) return;

    try {
      setIsSaving(true);
      if (editingCategory) {
        await knowledgeService.updateCategory(editingCategory.id, { name });
      } else {
        await knowledgeService.createCategory({ name });
      }
      await fetchCategories();
      setIsModalOpen(false);
    } catch (err) {
      console.error(err);
      alert('حدث خطأ أثناء الحفظ.');
    } finally {
      setIsSaving(false);
    }
  };

  const handleDeleteClick = (id: number) => {
    setDeletingId(id);
    setIsDeleteOpen(true);
  };

  const handleConfirmDelete = async () => {
    if (deletingId === null) return;
    try {
      setIsDeleting(true);
      await knowledgeService.deleteCategory(deletingId);
      await fetchCategories();
      setIsDeleteOpen(false);
    } catch (err) {
      console.error(err);
      alert('حدث خطأ أثناء الحذف.');
    } finally {
      setIsDeleting(false);
      setDeletingId(null);
    }
  };

  const columns = [
    { key: 'id', header: 'المعرف' },
    { key: 'name', header: 'الاسم' },
    {
      key: 'actions',
      header: 'الإجراءات',
      cell: (item: Category) => (
        <div className="flex gap-2">
          <Button variant="outline" size="sm" onClick={() => handleOpenModal(item)}>
            <Edit2 className="w-4 h-4 mr-1" />
            تعديل
          </Button>
          <Button variant="danger" size="sm" onClick={() => handleDeleteClick(item.id)}>
            <Trash2 className="w-4 h-4 mr-1" />
            حذف
          </Button>
        </div>
      ),
    },
  ];

  if (loading) return <LoadingState />;
  if (error) return <ErrorState message={error} onRetry={fetchCategories} />;

  return (
    <div className="space-y-6" dir="rtl">
      <div className="flex justify-between items-center">
        <h1 className="text-2xl font-bold">التصنيفات</h1>
        <Button onClick={() => handleOpenModal()}>
          <Plus className="w-4 h-4 ml-2" />
          إضافة تصنيف
        </Button>
      </div>

      <DataTable data={categories} columns={columns} />

      <Modal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editingCategory ? 'تعديل تصنيف' : 'إضافة تصنيف'}
      >
        <form onSubmit={handleSave} className="space-y-4">
          <Input
            label="اسم التصنيف"
            value={name}
            onChange={(e) => setName(e.target.value)}
            required
          />
          <div className="flex justify-end gap-2">
            <Button
              type="button"
              variant="outline"
              onClick={() => setIsModalOpen(false)}
              disabled={isSaving}
            >
              إلغاء
            </Button>
            <Button type="submit" isLoading={isSaving}>
              حفظ
            </Button>
          </div>
        </form>
      </Modal>

      <ConfirmationDialog
        isOpen={isDeleteOpen}
        onClose={() => setIsDeleteOpen(false)}
        onConfirm={handleConfirmDelete}
        title="تأكيد الحذف"
        message="هل أنت متأكد أنك تريد حذف هذا التصنيف؟"
        confirmText="حذف"
        cancelText="إلغاء"
        isLoading={isDeleting}
      />
    </div>
  );
}

import { useEffect, useState } from 'react';
import { Button } from '../components/Button';
import { DataTable } from '../components/DataTable';
import { SearchBox } from '../components/SearchBox';
import { Modal } from '../components/Modal';
import { Input } from '../components/Input';
import { LoadingState } from '../components/LoadingState';
import { ErrorState } from '../components/ErrorState';
import { ConfirmationDialog } from '../components/ConfirmationDialog';
import { ErrorDialog } from '../components/ErrorDialog';
import knowledgeService, { Category } from '../services/knowledgeService';
import { Plus, Edit2, Trash2 } from 'lucide-react';
import toast from 'react-hot-toast';

export function CategoriesPage() {
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingCategory, setEditingCategory] = useState<Category | null>(null);
  const [name, setName] = useState('');
  const [isSaving, setIsSaving] = useState(false);

  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [deletingId, setDeletingId] = useState<string | number | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);

  const [errorDialog, setErrorDialog] = useState<{
    isOpen: boolean;
    title?: string;
    message: string;
  }>({
    isOpen: false,
    message: '',
  });

  const showError = (message: string, title?: string) => {
    setErrorDialog({
      isOpen: true,
      title: title || 'حدث خطأ',
      message,
    });
    toast.error(message);
  };

  useEffect(() => {
    const timer = setTimeout(() => {
      fetchCategories(page, search);
    }, 300);
    return () => clearTimeout(timer);
  }, [page, search]);

  const fetchCategories = async (currentPage = page, currentSearch = search) => {
    try {
      setLoading(true);
      const data = await knowledgeService.getPagedCategories(currentPage, 10, currentSearch);
      setCategories(data.items);
      setTotalPages(data.totalPages);
      setTotalCount(data.totalCount);
      setError(null);
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
    if (!name.trim()) {
      showError('يرجى كتابة اسم التصنيف.', 'حقل مطلوب');
      return;
    }

    try {
      setIsSaving(true);
      if (editingCategory) {
        await knowledgeService.updateCategory(editingCategory.id, { name });
        toast.success('تم تحديث التصنيف بنجاح');
      } else {
        await knowledgeService.createCategory({ name });
        toast.success('تم إنشاء التصنيف بنجاح');
      }
      await fetchCategories();
      setIsModalOpen(false);
    } catch (err: any) {
      console.error(err);
      const msg = err.response?.data?.message || 'حدث خطأ أثناء حفظ التصنيف.';
      showError(msg, 'فشل الحفظ');
    } finally {
      setIsSaving(false);
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
      await knowledgeService.deleteCategory(deletingId);
      toast.success('تم حذف التصنيف بنجاح');
      await fetchCategories();
      setIsDeleteOpen(false);
    } catch (err: any) {
      console.error(err);
      const msg = err.response?.data?.message || 'حدث خطأ أثناء محاولة حذف التصنيف.';
      showError(msg, 'فشل الحذف');
    } finally {
      setIsDeleting(false);
      setDeletingId(null);
    }
  };

  const columns = [
    { 
      key: 'name', 
      header: 'اسم التصنيف',
      cell: (item: Category) => (
        <span className="font-semibold text-gray-900">{item.name}</span>
      )
    },
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

      <div className="max-w-md">
        <SearchBox
          value={search}
          onChange={(e) => {
            setSearch(e.target.value);
            setPage(1);
          }}
          placeholder="ابحث عن تصنيف..."
        />
      </div>

      <DataTable
        data={categories}
        columns={columns}
        currentPage={page}
        totalPages={totalPages}
        onPageChange={setPage}
      />

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

      <ErrorDialog
        isOpen={errorDialog.isOpen}
        onClose={() => setErrorDialog((prev) => ({ ...prev, isOpen: false }))}
        title={errorDialog.title}
        message={errorDialog.message}
      />
    </div>
  );
}

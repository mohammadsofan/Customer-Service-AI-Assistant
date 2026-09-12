import { useEffect, useState } from 'react';
import { Button } from '../components/Button';
import { DataTable } from '../components/DataTable';
import { Modal } from '../components/Modal';
import { Input } from '../components/Input';
import { LoadingState } from '../components/LoadingState';
import { ErrorState } from '../components/ErrorState';
import { ConfirmationDialog } from '../components/ConfirmationDialog';
import { ErrorDialog } from '../components/ErrorDialog';
import knowledgeService, { Keyword } from '../services/knowledgeService';
import { Plus, Trash2 } from 'lucide-react';
import toast from 'react-hot-toast';

export function KeywordsPage() {
  const [keywords, setKeywords] = useState<Keyword[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [word, setWord] = useState('');
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
    fetchKeywords();
  }, []);

  const fetchKeywords = async () => {
    try {
      setLoading(true);
      const data = await knowledgeService.getKeywords();
      setKeywords(data);
    } catch (err) {
      setError('فشل في جلب الكلمات المفتاحية.');
    } finally {
      setLoading(false);
    }
  };

  const handleOpenModal = () => {
    setWord('');
    setIsModalOpen(true);
  };

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!word.trim()) {
      showError('يرجى إدخال الكلمة المفتاحية.', 'حقل مطلوب');
      return;
    }

    try {
      setIsSaving(true);
      await knowledgeService.createKeyword({ word: word.trim() });
      toast.success('تمت إضافة الكلمة المفتاحية بنجاح');
      await fetchKeywords();
      setIsModalOpen(false);
    } catch (err: any) {
      console.error(err);
      const msg = err.response?.data?.message || 'حدث خطأ أثناء حفظ الكلمة المفتاحية.';
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
      await knowledgeService.deleteKeyword(deletingId);
      toast.success('تم حذف الكلمة المفتاحية بنجاح');
      await fetchKeywords();
      setIsDeleteOpen(false);
    } catch (err: any) {
      console.error(err);
      const msg = err.response?.data?.message || 'حدث خطأ أثناء محاولة حذف الكلمة المفتاحية.';
      showError(msg, 'فشل الحذف');
    } finally {
      setIsDeleting(false);
      setDeletingId(null);
    }
  };

  const columns = [
    { key: 'id', header: 'المعرف' },
    { key: 'word', header: 'الكلمة' },
    {
      key: 'actions',
      header: 'الإجراءات',
      cell: (item: Keyword) => (
        <div className="flex gap-2">
          <Button variant="danger" size="sm" onClick={() => handleDeleteClick(item.id)}>
            <Trash2 className="w-4 h-4 mr-1" />
            حذف
          </Button>
        </div>
      ),
    },
  ];

  if (loading) return <LoadingState />;
  if (error) return <ErrorState message={error} onRetry={fetchKeywords} />;

  return (
    <div className="space-y-6" dir="rtl">
      <div className="flex justify-between items-center">
        <h1 className="text-2xl font-bold">الكلمات المفتاحية</h1>
        <Button onClick={handleOpenModal}>
          <Plus className="w-4 h-4 ml-2" />
          إضافة كلمة مفتاحية +
        </Button>
      </div>

      <DataTable data={keywords} columns={columns} />

      <Modal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title="إضافة كلمة مفتاحية"
      >
        <form onSubmit={handleSave} className="space-y-4">
          <Input
            label="الكلمة"
            value={word}
            onChange={(e) => setWord(e.target.value)}
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
        message="هل أنت متأكد أنك تريد حذف هذه الكلمة المفتاحية؟"
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

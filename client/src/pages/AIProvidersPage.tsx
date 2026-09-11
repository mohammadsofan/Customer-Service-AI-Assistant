import React, { useState, useEffect } from 'react';
import { Button } from '../components/Button';
import { DataTable, type Column } from '../components/DataTable';
import { Modal } from '../components/Modal';
import { Input } from '../components/Input';
import { Select } from '../components/Select';
import { StatusBadge } from '../components/StatusBadge';
import { toast } from 'react-hot-toast';
import aiService, { type AiProvider } from '../services/aiService';

export function AIProvidersPage() {
  const [providers, setProviders] = useState<AiProvider[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [currentProvider, setCurrentProvider] = useState<AiProvider | null>(null);

  const [name, setName] = useState('');
  const [type, setType] = useState('OpenAI');
  const [baseUrl, setBaseUrl] = useState('');
  const [apiKey, setApiKey] = useState('');
  const [backupPriority, setBackupPriority] = useState('1');

  useEffect(() => {
    loadProviders();
  }, []);

  const loadProviders = async () => {
    try {
      setIsLoading(true);
      const data = await aiService.getProviders();
      setProviders(data);
    } catch (error) {
      toast.error('حدث خطأ أثناء تحميل مزودي الذكاء الاصطناعي');
    } finally {
      setIsLoading(false);
    }
  };

  const handleOpenModal = (provider?: AiProvider) => {
    if (provider) {
      setCurrentProvider(provider);
      setName(provider.name);
      setType(provider.providerType || 'OpenAI');
      setBaseUrl(provider.baseUrl || '');
      setApiKey('');
      setBackupPriority(provider.fallbackPriority?.toString() || '1');
    } else {
      setCurrentProvider(null);
      setName('');
      setType('OpenAI');
      setBaseUrl('');
      setApiKey('');
      setBackupPriority('1');
    }
    setIsModalOpen(true);
  };

  const handleSave = async () => {
    if (!name) {
      toast.error('يرجى كتابة اسم المزود');
      return;
    }
    if (!currentProvider && !apiKey) {
      toast.error('يرجى إدخال مفتاح API');
      return;
    }

    try {
      setIsSaving(true);
      if (currentProvider) {
        await aiService.updateProvider(currentProvider.id, {
          name,
          apiKey: apiKey.trim() ? apiKey.trim() : undefined,
          baseUrl: baseUrl.trim() ? baseUrl.trim() : undefined,
          fallbackPriority: Number(backupPriority) || 1
        });
        toast.success('تم تحديث المزود بنجاح');
      } else {
        await aiService.createProvider({
          name: name.trim(),
          providerType: type,
          apiKey: apiKey.trim(),
          baseUrl: baseUrl.trim() ? baseUrl.trim() : undefined,
          fallbackPriority: Number(backupPriority) || 1
        });
        toast.success('تمت إضافة المزود بنجاح');
      }
      setIsModalOpen(false);
      await loadProviders();
    } catch (error: any) {
      const msg = error.response?.data?.message || 'فشل حفظ المزود';
      toast.error(msg);
    } finally {
      setIsSaving(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (window.confirm('هل أنت متأكد من حذف هذا المزود؟')) {
      try {
        await aiService.deleteProvider(id);
        toast.success('تم الحذف بنجاح');
        await loadProviders();
      } catch (error) {
        toast.error('فشل حذف المزود');
      }
    }
  };

  const handleToggleActive = async (provider: AiProvider) => {
    try {
      if (provider.isActive) {
        await aiService.deactivateProvider(provider.id);
        toast.success('تم تعطيل المزود');
      } else {
        await aiService.activateProvider(provider.id);
        toast.success('تم تفعيل المزود');
      }
      await loadProviders();
    } catch (error) {
      toast.error('فشل تغيير حالة المزود');
    }
  };

  const handleTestConnection = async (provider: AiProvider) => {
    try {
      toast.loading('جاري اختبار الاتصال...', { id: 'test-conn' });
      const success = await aiService.testConnection({ id: provider.id });
      if (success) {
        toast.success('تم الاتصال بنجاح بالمزود', { id: 'test-conn' });
      } else {
        toast.error('فشل الاتصال بالمزود، يرجى التحقق من المفتاح والرابط', { id: 'test-conn' });
      }
    } catch {
      toast.error('فشل اختبار الاتصال بالمزود', { id: 'test-conn' });
    }
  };

  const columns: Column<AiProvider>[] = [
    { key: 'name', header: 'اسم المزود' },
    { key: 'providerType', header: 'النوع' },
    { 
      key: 'baseUrl', 
      header: 'رابط الخادم (Base URL)',
      cell: (item) => (
        <span className="font-mono text-xs text-gray-600" dir="ltr">
          {item.baseUrl || 'https://api.openai.com/v1/'}
        </span>
      )
    },
    { key: 'fallbackPriority', header: 'الأولوية' },
    { 
      key: 'isActive', 
      header: 'الحالة',
      cell: (item) => <StatusBadge status={item.isActive ? 'نشط' : 'غير نشط'} />
    },
    {
      key: 'actions',
      header: 'الإجراءات',
      cell: (item) => (
        <div className="flex gap-2">
          <Button variant="outline" size="sm" onClick={() => handleTestConnection(item)}>اختبار الاتصال</Button>
          <Button variant="secondary" size="sm" onClick={() => handleToggleActive(item)}>
            {item.isActive ? 'تعطيل' : 'تفعيل'}
          </Button>
          <Button variant="outline" size="sm" onClick={() => handleOpenModal(item)}>تعديل</Button>
          <Button variant="danger" size="sm" onClick={() => handleDelete(item.id)}>حذف</Button>
        </div>
      )
    }
  ];

  return (
    <div className="p-6" dir="rtl">
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">مزودي الذكاء الاصطناعي</h1>
          <p className="text-sm text-gray-500 mt-1">إدارة وبوابات مزودي خدمات الذكاء الاصطناعي والخوادم المتوافقة</p>
        </div>
        <Button onClick={() => handleOpenModal()}>إضافة مزود جديد</Button>
      </div>

      {isLoading ? (
        <div className="text-center py-8 text-gray-500">جاري التحميل...</div>
      ) : (
        <DataTable data={providers} columns={columns} />
      )}

      <Modal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={currentProvider ? 'تعديل المزود' : 'إضافة مزود جديد'}
      >
        <div className="space-y-4">
          <Input
            label="اسم المزود"
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="مثال: JustWoker AI أو OpenAI Primary"
          />
          <Select
            label="النوع"
            value={type}
            onChange={(e) => setType(e.target.value)}
            options={[
              { value: 'OpenAI', label: 'OpenAI (أو متوافق معه)' },
              { value: 'Custom', label: 'Custom' },
              { value: 'Anthropic', label: 'Anthropic' },
              { value: 'Gemini', label: 'Gemini' },
              { value: 'AzureOpenAI', label: 'Azure OpenAI' }
            ]}
          />
          <Input
            label="رابط الخادم المخصص (Base URL)"
            value={baseUrl}
            onChange={(e) => setBaseUrl(e.target.value)}
            placeholder="مثال: https://api.justwoker.icu/v1/"
            helperText="اتركه فارغاً للافتراضي الخاص بـ OpenAI، أو أدخل عنوان البروكسي/الخادم المخصص مثل https://api.justwoker.icu/v1/"
          />
          <Input
            label={currentProvider ? "مفتاح API (اتركه فارغاً إذا لم ترغب بتغييره)" : "مفتاح API"}
            type="password"
            value={apiKey}
            onChange={(e) => setApiKey(e.target.value)}
            placeholder="sk-..."
          />
          <Input
            label="أولوية النسخ الاحتياطي (Fallback Priority)"
            type="number"
            value={backupPriority}
            onChange={(e) => setBackupPriority(e.target.value)}
            min="1"
          />
          <div className="mt-6 flex justify-end gap-3">
            <Button variant="outline" onClick={() => setIsModalOpen(false)}>
              إلغاء
            </Button>
            <Button onClick={handleSave} isLoading={isSaving}>
              حفظ المزود
            </Button>
          </div>
        </div>
      </Modal>
    </div>
  );
}

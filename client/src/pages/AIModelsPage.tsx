import React, { useState, useEffect } from 'react';
import { Button } from '../components/Button';
import { DataTable, type Column } from '../components/DataTable';
import { Modal } from '../components/Modal';
import { Input } from '../components/Input';
import { Select } from '../components/Select';
import { toast } from 'react-hot-toast';
import aiService, { type AiModel, type AiProvider } from '../services/aiService';

export function AIModelsPage() {
  const [models, setModels] = useState<AiModel[]>([]);
  const [providers, setProviders] = useState<AiProvider[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  
  const [filterProviderId, setFilterProviderId] = useState('');
  
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [currentModel, setCurrentModel] = useState<AiModel | null>(null);

  const [name, setName] = useState('');
  const [providerId, setProviderId] = useState('');
  const [isEmbeddingModel, setIsEmbeddingModel] = useState(false);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setIsLoading(true);
      const provs = await aiService.getProviders();
      setProviders(provs);
      
      let allModels: AiModel[] = [];
      if (provs.length > 0) {
         // Load models for the first provider or all, assuming all for now by iterating
         for (const p of provs) {
           const pModels = await aiService.getModels(p.id);
           allModels = [...allModels, ...pModels];
         }
      }
      setModels(allModels);
    } catch (error) {
      toast.error('حدث خطأ أثناء تحميل البيانات');
    } finally {
      setIsLoading(false);
    }
  };

  const handleOpenModal = (model?: AiModel) => {
    if (model) {
      setCurrentModel(model);
      setName(model.name || model.modelName || '');
      setProviderId(model.providerId);
      setIsEmbeddingModel(model.isEmbeddingModel || false);
    } else {
      setCurrentModel(null);
      setName('');
      setProviderId(providers[0]?.id || '');
      setIsEmbeddingModel(false);
    }
    setIsModalOpen(true);
  };

  const [isSaving, setIsSaving] = useState(false);

  const handleSave = async () => {
    if (!name || !providerId) {
      toast.error('يرجى تعبئة جميع الحقول المطلوبة');
      return;
    }
    
    try {
      setIsSaving(true);
      if (currentModel) {
        await aiService.updateModel(currentModel.id, name.trim(), isEmbeddingModel);
        toast.success('تم تحديث النموذج بنجاح');
      } else {
        await aiService.createModel({
          providerId,
          modelName: name.trim(),
          isEmbeddingModel
        });
        toast.success('تمت إضافة النموذج بنجاح');
      }
      setIsModalOpen(false);
      await loadData();
    } catch (error: any) {
      const msg = error.response?.data?.message || error.response?.data?.title || error.message || 'فشل حفظ النموذج';
      toast.error(msg);
    } finally {
      setIsSaving(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (window.confirm('هل أنت متأكد من حذف هذا النموذج؟')) {
      try {
        await aiService.deleteModel(id);
        toast.success('تم الحذف بنجاح');
        await loadData();
      } catch (error: any) {
        const msg = error.response?.data?.message || error.response?.data?.title || error.message || 'فشل حذف النموذج';
        toast.error(msg);
      }
    }
  };

  const activeModels = models.filter(m => m.isActive !== false);
  const filteredModels = filterProviderId 
    ? activeModels.filter(m => m.providerId === filterProviderId)
    : activeModels;

  const columns: Column<AiModel>[] = [
    { 
      key: 'name', 
      header: 'اسم النموذج',
      cell: (item) => (
        <div className="flex items-center gap-2">
          <span>{item.name}</span>
          {item.isEmbeddingModel && (
            <span className="bg-blue-100 text-blue-800 text-xs font-medium px-2.5 py-0.5 rounded border border-blue-400">
              Embedding
            </span>
          )}
        </div>
      )
    },
    { 
      key: 'providerId', 
      header: 'المزود',
      cell: (item) => providers.find(p => p.id === item.providerId)?.name || item.providerId
    },
    {
      key: 'actions',
      header: 'الإجراءات',
      cell: (item) => (
        <div className="flex gap-2">
          <Button variant="outline" onClick={() => handleOpenModal(item)}>تعديل</Button>
          <Button variant="danger" onClick={() => handleDelete(item.id)}>حذف</Button>
        </div>
      )
    }
  ];

  return (
    <div className="p-6" dir="rtl">
      <div className="mb-6 flex flex-col sm:flex-row items-center justify-between gap-4">
        <h1 className="text-2xl font-bold text-gray-900">نماذج الذكاء الاصطناعي</h1>
        
        <div className="flex items-center gap-4 w-full sm:w-auto">
          <div className="w-48">
            <Select
              options={[{ value: '', label: 'جميع المزودين' }, ...providers.map(p => ({ value: p.id, label: p.name }))]}
              value={filterProviderId}
              onChange={(e) => setFilterProviderId(e.target.value)}
            />
          </div>
          <Button onClick={() => handleOpenModal()}>إضافة نموذج جديد</Button>
        </div>
      </div>

      {isLoading ? (
        <div>جاري التحميل...</div>
      ) : (
        <DataTable data={filteredModels} columns={columns} />
      )}

      <Modal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={currentModel ? 'تعديل نموذج' : 'إضافة نموذج جديد'}
      >
        <div className="space-y-4">
          <Input
            label="اسم النموذج"
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="مثال: gpt-4"
          />
          <Select
            label="المزود"
            value={providerId}
            onChange={(e) => setProviderId(e.target.value)}
            options={providers.map(p => ({ value: p.id, label: p.name }))}
          />
          <div className="flex items-center gap-2 mt-4">
            <input
              type="checkbox"
              id="isEmbeddingModel"
              checked={isEmbeddingModel}
              onChange={(e) => setIsEmbeddingModel(e.target.checked)}
              className="w-4 h-4 text-primary bg-gray-100 border-gray-300 rounded focus:ring-primary focus:ring-2"
            />
            <label htmlFor="isEmbeddingModel" className="text-sm font-medium text-gray-900">
              نموذج تضمين وبحث (Embedding Model)
            </label>
          </div>
          <div className="mt-6 flex justify-end gap-3">
            <Button variant="outline" onClick={() => setIsModalOpen(false)}>
              إلغاء
            </Button>
            <Button onClick={handleSave} isLoading={isSaving}>
              حفظ
            </Button>
          </div>
        </div>
      </Modal>
    </div>
  );
}

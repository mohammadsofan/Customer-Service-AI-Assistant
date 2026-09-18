import React, { useState, useEffect } from 'react';
import { Button } from '../components/Button';
import { Select } from '../components/Select';
import { Input } from '../components/Input';
import { Textarea } from '../components/Textarea';
import { toast } from 'react-hot-toast';
import aiService, { type AiProvider, type AiModel, type AiConfiguration } from '../services/aiService';

export function AIConfigurationPage() {
  const [providers, setProviders] = useState<AiProvider[]>([]);
  const [models, setModels] = useState<AiModel[]>([]);
  const [embeddingModels, setEmbeddingModels] = useState<AiModel[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [isTesting, setIsTesting] = useState(false);

  const [config, setConfig] = useState<AiConfiguration>({
    providerId: '',
    modelId: '',
    apiKey: '',
    isActive: true,
  });

  const [temperature, setTemperature] = useState('0.7');
  const [maxTokens, setMaxTokens] = useState('2000');
  const [similarityThreshold, setSimilarityThreshold] = useState('0.85');
  const [topK, setTopK] = useState('3');
  const [llmRerankingEnabled, setLlmRerankingEnabled] = useState(false);
  const [llmRerankingTopK, setLlmRerankingTopK] = useState('10');
  const [llmRerankingConfidenceThreshold, setLlmRerankingConfidenceThreshold] = useState('0.6');
  const [systemPrompt, setSystemPrompt] = useState('أنت مساعد ذكي لخدمة العملاء...');
  const [autoFailover, setAutoFailover] = useState(true);

  // For embedding tracking
  const [initialEmbeddingModelId, setInitialEmbeddingModelId] = useState<string | null>(null);
  const [isConfirmOpen, setIsConfirmOpen] = useState(false);
  const [isProgressOpen, setIsProgressOpen] = useState(false);
  const [stats, setStats] = useState<{ totalScenarios: number, pendingEmbeddings: number, readyEmbeddings: number, failedEmbeddings: number } | null>(null);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setIsLoading(true);
      const [provs, configuration] = await Promise.all([
        aiService.getProviders(),
        aiService.getConfiguration().catch(() => null)
      ]);
      setProviders(provs);
      
      if (configuration) {
        setConfig(configuration);
        setTemperature(configuration.temperature?.toString() || '0.7');
        setMaxTokens(configuration.maxTokens?.toString() || '2000');
        setSimilarityThreshold(configuration.similarityThreshold?.toString() || '0.85');
        setTopK(configuration.topK?.toString() || '3');
        setLlmRerankingEnabled(configuration.llmRerankingEnabled ?? false);
        setLlmRerankingTopK(configuration.llmRerankingTopK?.toString() || '10');
        setLlmRerankingConfidenceThreshold(configuration.llmRerankingConfidenceThreshold?.toString() || '0.6');
        setSystemPrompt(configuration.systemPrompt || '');
        setAutoFailover(configuration.enableAutoFailover ?? true);
        
        setInitialEmbeddingModelId(configuration.activeEmbeddingModelId || null);

        const activeProvId = configuration.activeProviderId || configuration.providerId;
        if (activeProvId) {
          const provModels = await aiService.getModels(activeProvId);
          setModels(provModels.filter(m => !m.isEmbeddingModel));
        }
        if (configuration.activeEmbeddingProviderId) {
          const embModels = await aiService.getModels(configuration.activeEmbeddingProviderId);
          setEmbeddingModels(embModels.filter(m => m.isEmbeddingModel));
        }
      }
    } catch (error) {
      toast.error('حدث خطأ أثناء تحميل الإعدادات');
    } finally {
      setIsLoading(false);
    }
  };

  const handleProviderChange = async (providerId: string) => {
    setConfig({ ...config, providerId, activeProviderId: providerId, modelId: '', activeModelId: '' });
    try {
      if (providerId) {
        const provModels = await aiService.getModels(providerId);
        setModels(provModels.filter(m => !m.isEmbeddingModel));
      } else {
        setModels([]);
      }
    } catch {
      toast.error('خطأ في تحميل النماذج');
    }
  };

  const handleSave = () => {
    const activeProvId = config.activeProviderId || config.providerId;
    const activeModId = config.activeModelId || config.modelId;
    if (!activeProvId) {
      toast.error('يرجى اختيار المزود النشط');
      return;
    }
    if (!activeModId) {
      toast.error('يرجى اختيار النموذج النشط');
      return;
    }

    const hasEmbeddingChanged = initialEmbeddingModelId !== config.activeEmbeddingModelId;
    if (hasEmbeddingChanged) {
      setIsConfirmOpen(true);
    } else {
      executeSave();
    }
  };

  const executeSave = async () => {
    const activeProvId = config.activeProviderId || config.providerId;
    const activeModId = config.activeModelId || config.modelId;
    
    try {
      setIsSaving(true);
      await aiService.updateConfiguration({
        providerId: config.providerId,
        activeProviderId: config.activeProviderId,
        modelId: config.modelId,
        activeModelId: activeModId,
        activeEmbeddingProviderId: config.activeEmbeddingProviderId,
        activeEmbeddingModelId: config.activeEmbeddingModelId,
        temperature: parseFloat(temperature) || 0.7,
        maxTokens: parseInt(maxTokens) || 1024,
        similarityThreshold: parseFloat(similarityThreshold) || 0.75,
        topK: parseInt(topK) || 5,
        llmRerankingEnabled,
        llmRerankingTopK: parseInt(llmRerankingTopK) || 10,
        llmRerankingConfidenceThreshold: parseFloat(llmRerankingConfidenceThreshold) || 0.6,
        systemPrompt,
        enableAutoFailover: autoFailover
      });
      
      const hasEmbeddingChanged = initialEmbeddingModelId !== config.activeEmbeddingModelId;
      setInitialEmbeddingModelId(config.activeEmbeddingModelId || null);
      
      toast.success('تم حفظ الإعدادات بنجاح');
      
      if (hasEmbeddingChanged) {
        setIsConfirmOpen(false);
        setIsProgressOpen(true);
        pollStats();
      }
    } catch {
      toast.error('حدث خطأ أثناء الحفظ');
    } finally {
      setIsSaving(false);
    }
  };

  const pollStats = async () => {
    try {
      const currentStats = await aiService.getEmbeddingStats();
      setStats(currentStats);
      if (currentStats.pendingEmbeddings > 0) {
        setTimeout(pollStats, 2000);
      } else {
        setTimeout(() => setIsProgressOpen(false), 2000);
        toast.success('تم الانتهاء من إعادة تضمين جميع السيناريوهات');
      }
    } catch (e) {
      setTimeout(pollStats, 2000);
    }
  };

  const handleTestConnection = async () => {
    const activeProvId = config.activeProviderId || config.providerId;
    if (!activeProvId) {
      toast.error('يرجى اختيار المزود أولاً لاختبار الاتصال');
      return;
    }
    try {
      setIsTesting(true);
      const res = await aiService.testConnection({ activeProviderId: activeProvId });
      if (res.success) {
        toast.success(res.message || 'تم الاتصال بنجاح بالمزود');
      } else {
        toast.error(res.message || 'فشل الاتصال بالمزود', { duration: 7000 });
      }
    } catch (err: any) {
      toast.error(err?.message || 'فشل الاتصال بالمزود');
    } finally {
      setIsTesting(false);
    }
  };

  if (isLoading) {
    return <div className="p-6" dir="rtl">جاري التحميل...</div>;
  }

  return (
    <div className="p-6 max-w-4xl mx-auto" dir="rtl">
      <h1 className="text-2xl font-bold text-gray-900 mb-6">إعدادات الذكاء الاصطناعي العامة</h1>
      
      <div className="bg-white rounded-lg shadow border border-gray-200 p-6 space-y-6">
        
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <Select
            label="المزود النشط (للإجابات)"
            value={config.activeProviderId || config.providerId || ''}
            onChange={(e) => handleProviderChange(e.target.value)}
            options={[{ value: '', label: 'اختر المزود' }, ...providers.map(p => ({ value: p.id, label: p.name }))]}
          />
          <Select
            label="النموذج النشط (للإجابات)"
            value={config.activeModelId || config.modelId || ''}
            onChange={(e) => setConfig({ ...config, modelId: e.target.value, activeModelId: e.target.value })}
            options={[{ value: '', label: 'اختر النموذج' }, ...models.map(m => ({ value: m.id, label: m.name || m.modelName || 'Model' }))]}
            disabled={!(config.activeProviderId || config.providerId)}
          />
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <Select
            label="مزود التضمين والبحث (Embedding)"
            value={config.activeEmbeddingProviderId || ''}
            onChange={async (e) => {
              const pId = e.target.value;
              setConfig({ ...config, activeEmbeddingProviderId: pId, activeEmbeddingModelId: '' });
              if (pId) {
                try {
                  const embModels = await aiService.getModels(pId);
                  setEmbeddingModels(embModels.filter(m => m.isEmbeddingModel));
                } catch {
                  toast.error('خطأ في تحميل نماذج التضمين');
                }
              } else {
                setEmbeddingModels([]);
              }
            }}
            options={[{ value: '', label: 'بدون (استخدام الإعدادات الافتراضية)' }, ...providers.map(p => ({ value: p.id, label: p.name }))]}
          />
          <Select
            label="نموذج التضمين (Embedding)"
            value={config.activeEmbeddingModelId || ''}
            onChange={(e) => setConfig({ ...config, activeEmbeddingModelId: e.target.value })}
            options={[{ value: '', label: 'اختر النموذج' }, ...embeddingModels.map(m => ({ value: m.id, label: m.name || m.modelName || 'Model' }))]}
            disabled={!config.activeEmbeddingProviderId}
          />
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <Input
            label="درجة الإبداع (Temperature)"
            type="number"
            step="0.1"
            min="0"
            max="2"
            value={temperature}
            onChange={(e) => setTemperature(e.target.value)}
          />
          <Input
            label="الحد الأقصى للكلمات (Max Tokens)"
            type="number"
            value={maxTokens}
            onChange={(e) => setMaxTokens(e.target.value)}
          />
          <Input
            label="عتبة التشابه (Similarity Threshold)"
            type="number"
            step="0.05"
            min="0"
            max="1"
            value={similarityThreshold}
            onChange={(e) => setSimilarityThreshold(e.target.value)}
          />
            <Input
              label="أفضل النتائج للبحث الرياضي (Retrieval Top K)"
              type="number"
              value={topK}
              onChange={(e) => setTopK(e.target.value)}
            />
          </div>

          <div className="border-t pt-4 mt-4">
            <h3 className="text-lg font-medium mb-4">إعدادات إعادة الترتيب الذكي (LLM Reranking)</h3>
            
            <div className="flex items-center gap-3 mb-4">
              <input
                type="checkbox"
                id="llmRerankingEnabled"
                checked={llmRerankingEnabled}
                onChange={(e) => setLlmRerankingEnabled(e.target.checked)}
                className="w-4 h-4 text-primary"
              />
              <label htmlFor="llmRerankingEnabled" className="font-medium">
                تفعيل إعادة الترتيب بالذكاء الاصطناعي (Two-Stage RAG)
              </label>
            </div>

            {llmRerankingEnabled && (
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <Input
                  label="عدد السيناريوهات للمرشحين (LLM Reranking Top K)"
                  type="number"
                  value={llmRerankingTopK}
                  onChange={(e) => setLlmRerankingTopK(e.target.value)}
                />
                <Input
                  label="عتبة الثقة (Confidence Threshold)"
                  type="number"
                  step="0.05"
                  min="0"
                  max="1"
                  value={llmRerankingConfidenceThreshold}
                  onChange={(e) => setLlmRerankingConfidenceThreshold(e.target.value)}
                />
              </div>
            )}
          </div>

        <div>
          <Textarea
            label="موجه النظام (System Prompt)"
            value={systemPrompt}
            onChange={(e) => setSystemPrompt(e.target.value)}
            rows={4}
          />
        </div>

        <div className="flex items-center gap-3">
          <input
            type="checkbox"
            id="autoFailover"
            checked={autoFailover}
            onChange={(e) => setAutoFailover(e.target.checked)}
            className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
          />
          <label htmlFor="autoFailover" className="text-sm font-medium text-gray-700">
            تفعيل الانتقال التلقائي لمزود احتياطي عند الفشل (Auto Failover)
          </label>
        </div>

        <div className="pt-4 border-t border-gray-200 flex gap-4">
          <Button onClick={handleSave} isLoading={isSaving}>
            حفظ الإعدادات
          </Button>
          <Button variant="outline" onClick={handleTestConnection} isLoading={isTesting}>
            اختبار الاتصال
          </Button>
        </div>
      </div>

      {isConfirmOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
          <div className="fixed inset-0 bg-black/30" onClick={() => setIsConfirmOpen(false)}></div>
          <div className="relative mx-auto max-w-sm rounded bg-white p-6 shadow-xl" dir="rtl">
            <h2 className="text-lg font-bold mb-4 text-red-600">تنبيه تغيير نموذج التضمين</h2>
            <p className="mb-6 text-gray-600">
              تغيير نموذج التضمين (Embedding Model) سيؤدي إلى <strong>إعادة تضمين جميع السيناريوهات</strong> الموجودة في قاعدة البيانات باستخدام النموذج الجديد لضمان توافق البحث.
              <br /><br />
              قد تستغرق هذه العملية بعض الوقت وتستهلك رصيداً من المزود الجديد. هل أنت متأكد من المتابعة؟
            </p>
            <div className="flex gap-4">
              <Button onClick={executeSave} isLoading={isSaving} className="bg-red-600 hover:bg-red-700">نعم، متأكد</Button>
              <Button variant="outline" onClick={() => setIsConfirmOpen(false)} disabled={isSaving}>إلغاء</Button>
            </div>
          </div>
        </div>
      )}

      {isProgressOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
          <div className="fixed inset-0 bg-black/30"></div>
          <div className="relative mx-auto max-w-md rounded bg-white p-6 w-full shadow-xl" dir="rtl">
            <h2 className="text-lg font-bold mb-4">جاري إعادة التضمين...</h2>
            {stats ? (
              <div>
                <p className="text-gray-600 mb-2">
                  يتم الآن بناء متجهات التضمين باستخدام النموذج الجديد:
                </p>
                <div className="w-full bg-gray-200 rounded-full h-2.5 mb-4 overflow-hidden">
                  <div 
                    className="bg-blue-600 h-2.5 rounded-full transition-all duration-500" 
                    style={{ width: `${stats.totalScenarios > 0 ? (stats.readyEmbeddings / stats.totalScenarios) * 100 : 100}%` }}
                  ></div>
                </div>
                <div className="flex justify-between text-sm text-gray-600">
                  <span>المنجزة: {stats.readyEmbeddings}</span>
                  <span>المتبقية: {stats.pendingEmbeddings}</span>
                  <span>الإجمالي: {stats.totalScenarios}</span>
                </div>
                {stats.failedEmbeddings > 0 && (
                  <p className="text-red-500 text-sm mt-2">فشل: {stats.failedEmbeddings} (سيعاد المحاولة)</p>
                )}
              </div>
            ) : (
              <p className="text-gray-600">جاري الاتصال...</p>
            )}
          </div>
        </div>
      )}
    </div>
  );
}

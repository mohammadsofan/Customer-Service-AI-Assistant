function App() {
  return (
    <div className="min-h-screen bg-[var(--color-background)] p-8">
      <div className="max-w-2xl mx-auto bg-[var(--color-surface)] rounded-xl shadow-lg p-8">
        <h1 className="text-3xl font-bold text-[var(--color-primary)] mb-4">
          مساعد دعم الموظفين الذكي
        </h1>
        <p className="text-lg text-[var(--color-text-secondary)] mb-6">
          مرحباً بكم في نظام دعم الموظفين المدعوم بالذكاء الاصطناعي
        </p>
        <div className="flex gap-4">
          <button className="px-6 py-2 bg-[var(--color-primary)] text-white rounded-lg hover:bg-[var(--color-primary-hover)] transition-colors">
            تسجيل الدخول
          </button>
          <button className="px-6 py-2 border border-[var(--color-border)] text-[var(--color-text-primary)] rounded-lg hover:bg-[var(--color-background)] transition-colors">
            إنشاء حساب
          </button>
        </div>
      </div>
    </div>
  )
}

export default App

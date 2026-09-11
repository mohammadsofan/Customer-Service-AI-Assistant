const fs = require('fs');
const path = require('path');

const pages = [
  { name: 'LoginPage', arabic: 'تسجيل الدخول' },
  { name: 'EmployeeSupportPage', arabic: 'دعم الموظفين' },
  { name: 'AdminDashboard', arabic: 'الرئيسية' },
  { name: 'ScenarioListPage', arabic: 'قائمة السيناريوهات' },
  { name: 'CreateScenarioPage', arabic: 'إنشاء سيناريو' },
  { name: 'EditScenarioPage', arabic: 'تعديل سيناريو' },
  { name: 'CategoriesPage', arabic: 'الفئات' },
  { name: 'KeywordsPage', arabic: 'الكلمات المفتاحية' },
  { name: 'AIProvidersPage', arabic: 'موفرو الذكاء الاصطناعي' },
  { name: 'AIModelsPage', arabic: 'نماذج الذكاء الاصطناعي' },
  { name: 'AIConfigurationPage', arabic: 'إعدادات الذكاء الاصطناعي' },
  { name: 'EmployeesPage', arabic: 'الموظفون' },
  { name: 'QuestionsPage', arabic: 'الأسئلة' },
  { name: 'AnalyticsPage', arabic: 'الإحصائيات' },
  { name: 'AuditLogPage', arabic: 'سجل التدقيق' },
];

const dir = path.join(__dirname, 'src', 'pages');
if (!fs.existsSync(dir)){
    fs.mkdirSync(dir, { recursive: true });
}

pages.forEach(page => {
  const content = `export const ${page.name} = () => {
  return (
    <div className="p-4 bg-white rounded shadow">
      <h2 className="text-xl font-bold">${page.arabic}</h2>
    </div>
  );
};
`;
  fs.writeFileSync(path.join(dir, `${page.name}.tsx`), content);
});

console.log('Pages generated successfully!');

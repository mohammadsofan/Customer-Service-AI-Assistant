import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthLayout } from './layouts/AuthLayout';
import { EmployeeLayout } from './layouts/EmployeeLayout';
import { AdminLayout } from './layouts/AdminLayout';
import { ProtectedRoute } from './components/ProtectedRoute';
import { AdminRoute } from './components/AdminRoute';

import { 
  LoginPage,
  EmployeeSupportPage,
  AdminDashboard,
  ScenarioListPage,
  CreateScenarioPage,
  EditScenarioPage,
  CategoriesPage,
  KeywordsPage,
  AIProvidersPage,
  AIModelsPage,
  AIConfigurationPage,
  EmployeesPage,
  QuestionsPage,
  AnalyticsPage,
  AuditLogPage
} from './pages';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Public Routes */}
        <Route element={<AuthLayout />}>
          <Route path="/login" element={<LoginPage />} />
        </Route>

        {/* Protected Routes (Any Authenticated User) */}
        <Route element={<ProtectedRoute />}>
          {/* Employee Routes */}
          <Route element={<EmployeeLayout />}>
            <Route path="/support" element={<EmployeeSupportPage />} />
          </Route>

          {/* Admin Routes */}
          <Route element={<AdminRoute />}>
            <Route path="/admin" element={<AdminLayout />}>
              <Route index element={<AdminDashboard />} />
              <Route path="knowledge" element={<ScenarioListPage />} />
              <Route path="knowledge/create" element={<CreateScenarioPage />} />
              <Route path="knowledge/edit/:id" element={<EditScenarioPage />} />
              <Route path="knowledge/categories" element={<CategoriesPage />} />
              <Route path="knowledge/keywords" element={<KeywordsPage />} />
              
              <Route path="ai" element={<AIConfigurationPage />} />
              <Route path="ai/providers" element={<AIProvidersPage />} />
              <Route path="ai/models" element={<AIModelsPage />} />
              
              <Route path="employees" element={<EmployeesPage />} />
              <Route path="questions" element={<QuestionsPage />} />
              <Route path="analytics" element={<AnalyticsPage />} />
              <Route path="audit" element={<AuditLogPage />} />
            </Route>
          </Route>
        </Route>

        {/* Fallback routes */}
        <Route path="/" element={<Navigate to="/support" replace />} />
        <Route path="*" element={<Navigate to="/support" replace />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;

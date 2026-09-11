import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';

export const AdminRoute = () => {
  const { isAuthenticated, isAdmin, user } = useAuth();
  
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }
  
  const roleLower = (user?.role || '').toLowerCase();
  const hasAdminAccess = isAdmin || roleLower === 'admin' || roleLower === 'administrator';

  if (!hasAdminAccess) {
    return <Navigate to="/support" replace />;
  }

  return <Outlet />;
};

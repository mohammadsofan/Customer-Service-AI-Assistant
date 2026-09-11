import { Outlet } from 'react-router-dom';

export const AuthLayout = () => {
  return (
    <div 
      dir="rtl" 
      className="min-h-screen w-full flex items-center justify-center p-4 sm:p-6 lg:p-8 bg-gradient-to-br from-slate-900 via-indigo-950 to-slate-900 relative overflow-hidden"
    >
      {/* Ambient background glows */}
      <div className="absolute -top-40 -right-40 w-96 h-96 bg-blue-500/20 rounded-full blur-3xl pointer-events-none" />
      <div className="absolute -bottom-40 -left-40 w-96 h-96 bg-indigo-500/20 rounded-full blur-3xl pointer-events-none" />
      <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-full max-w-7xl h-full pointer-events-none bg-[radial-gradient(circle_at_center,rgba(59,130,246,0.05)_0,transparent_100%)]" />

      {/* Outlet */}
      <div className="w-full max-w-md relative z-10">
        <Outlet />
      </div>
    </div>
  );
};

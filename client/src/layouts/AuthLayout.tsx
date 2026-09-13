import { Outlet } from 'react-router-dom';

export const AuthLayout = () => {
  return (
    <div 
      dir="rtl" 
      className="min-h-screen w-full flex items-center justify-center p-4 sm:p-6 lg:p-8 bg-gradient-to-br from-slate-900 via-slate-950 to-[#002654] relative overflow-hidden"
    >
      {/* Ambient background glows */}
      <div className="absolute -top-36 -right-36 w-96 h-96 bg-[#76bc21]/20 rounded-full blur-3xl pointer-events-none" />
      <div className="absolute -bottom-36 -left-36 w-96 h-96 bg-[#0055b8]/30 rounded-full blur-3xl pointer-events-none" />
      <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-full max-w-7xl h-full pointer-events-none bg-[radial-gradient(circle_at_center,rgba(118,188,33,0.06)_0,transparent_100%)]" />

      {/* Outlet */}
      <div className="w-full max-w-md relative z-10">
        <Outlet />
      </div>
    </div>
  );
};

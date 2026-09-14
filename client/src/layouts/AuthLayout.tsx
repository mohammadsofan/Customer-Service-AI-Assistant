import { Outlet } from 'react-router-dom';

export const AuthLayout = () => {
  return (
    <div 
      dir="rtl" 
      className="min-h-screen w-full flex items-center justify-center p-4 sm:p-6 lg:p-8 bg-[#f5f5f7] relative overflow-hidden font-sans"
    >
      {/* Ambient background brand glows */}
      <div className="absolute -top-32 -right-32 w-96 h-96 bg-[#76bc21]/15 rounded-full blur-3xl pointer-events-none" />
      <div className="absolute -bottom-32 -left-32 w-96 h-96 bg-[#0055b8]/12 rounded-full blur-3xl pointer-events-none" />
      <div className="absolute top-1/4 -left-20 w-72 h-72 bg-[#f4771d]/8 rounded-full blur-3xl pointer-events-none" />
      <div className="absolute inset-0 bg-[radial-gradient(#0055b8_1px,transparent_1px)] [background-size:28px_28px] opacity-[0.035] pointer-events-none" />

      {/* Outlet */}
      <div className="w-full max-w-md relative z-10">
        <Outlet />
      </div>
    </div>
  );
};


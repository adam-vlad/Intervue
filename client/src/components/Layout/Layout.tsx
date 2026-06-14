import { Outlet } from 'react-router-dom';
import Navbar from '../Navbar/Navbar';
import Sidebar from '../Sidebar/Sidebar';
import './Layout.css';

export function HomeLayout() {
  return (
    <div className="layout">
      <Navbar />
      <main className="layout-content layout-content-full">
        <Outlet />
      </main>
    </div>
  );
}

export function AppLayout() {
  return (
    <div className="layout">
      <Navbar />
      <Sidebar />
      <main className="layout-content layout-content-sidebar">
        <Outlet />
      </main>
    </div>
  );
}

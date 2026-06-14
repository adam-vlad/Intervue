import { createBrowserRouter, Navigate } from 'react-router-dom';
import { HomeLayout, AppLayout } from './components/Layout/Layout';
import Home from './pages/Home/Home';
import Dashboard from './pages/Dashboard/Dashboard';
import Upload from './pages/Upload/Upload';
import Profile from './pages/Profile/Profile';
import Interview from './pages/Interview/Interview';
import Feedback from './pages/Feedback/Feedback';

export const router = createBrowserRouter([
  {
    element: <HomeLayout />,
    children: [
      { path: '/', element: <Home /> },
    ],
  },
  {
    element: <AppLayout />,
    children: [
      { path: '/dashboard', element: <Dashboard /> },
      { path: '/upload', element: <Upload /> },
      { path: '/profile/:id', element: <Profile /> },
      { path: '/interview/:id', element: <Interview /> },
      { path: '/interview/:id/feedback', element: <Feedback /> },
      { path: '*', element: <Navigate to="/" replace /> },
    ],
  },
]);

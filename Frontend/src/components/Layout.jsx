import React, { useState } from 'react';
import { Outlet } from 'react-router-dom';
import { Navbar } from './Navbar';
import { Sidebar } from './Sidebar';

export const Layout = () => {
  const [sidebarOpen, setSidebarOpen] = useState(false);

  const toggleSidebar = () => {
    setSidebarOpen((prev) => !prev);
  };

  const closeSidebar = () => {
    setSidebarOpen(false);
  };

  return (
    <div className="min-vh-100 bg-light d-flex flex-column">
      <Navbar onToggleSidebar={toggleSidebar} />
      <div className="d-flex flex-grow-1">
        <Sidebar isOpen={sidebarOpen} onClose={closeSidebar} />
        <main
          className="flex-grow-1 p-3 p-md-4"
          style={{
            marginLeft: '0px',
            maxWidth: '100%',
          }}
        >
          <div className="layout-content-wrapper">
            <Outlet />
          </div>
        </main>
      </div>
    </div>
  );
};

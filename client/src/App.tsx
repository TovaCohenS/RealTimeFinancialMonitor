
import './App.css'
import { BrowserRouter, NavLink, Route, Routes } from 'react-router-dom'
import { MonitorPage } from './pages/monitor/MonitorPage'
import { AddPage } from './pages/add/AddPage'
import { TransactionsLayout } from './routes/TransactionsLayout'

function App() {
  return (
    <BrowserRouter>
      <div style={{ padding: 16, borderBottom: "1px solid #eee", display: "flex", gap: 12 }}>
        <NavLink to="/add">Add</NavLink>
        <NavLink to="/monitor">Monitor</NavLink>
      </div>

      <Routes>
        <Route path="/add" element={<AddPage />} />

        <Route element={<TransactionsLayout />}>
          <Route path="/" element={<MonitorPage />} />
          <Route path="/monitor" element={<MonitorPage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  )
}

export default App

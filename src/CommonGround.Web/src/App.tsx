import './App.css'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import Layout from './components/Layout'
import BuildingsPage from './pages/BuildingsPage'
import GroundLeasesPage from './pages/GroundLeasesPage'
import LandParcelsPage from './pages/LandParcelsPage'
import PurchaseWorkflowsPage from './pages/PurchaseWorkflowsPage'

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<Layout />}>
          <Route index element={<Navigate to="/land-parcels" replace />} />
          <Route path="/land-parcels"         element={<LandParcelsPage />} />
          <Route path="/buildings"            element={<BuildingsPage />} />
          <Route path="/ground-leases"        element={<GroundLeasesPage />} />
          <Route path="/purchase-workflows"   element={<PurchaseWorkflowsPage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  )
}

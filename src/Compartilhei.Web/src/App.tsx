import { BrowserRouter, Route, Routes } from 'react-router-dom';

import Welcome from './pages/Welcome/Welcome';
import Albums from './pages/Albums/Albums';
import Favorites from './pages/Favorites/Favorites';
import Download from './pages/Download/Download';
import Gallery from "./pages/Gallery/Gallery";

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route
          path="/:eventSlug"
          element={<Welcome />}
        />

        <Route
          path="/:eventSlug/albuns"
          element={<Albums />}
        />

        <Route
          path="/:eventSlug/favoritos"
          element={<Favorites />}
        />

        <Route
          path="/:eventSlug/download"
          element={<Download />}
        />

        <Route
          path="/:eventSlug/albuns/:albumId"
          element={<Gallery />}
        />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
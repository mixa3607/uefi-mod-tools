import { createRoot } from 'react-dom/client';
import { App } from './App';
import './styles.css';

const dataElement = document.getElementById('ifr-data');
const rootElement = document.getElementById('root');

if (!dataElement?.textContent || !rootElement) {
  throw new Error('IFR viewer document data or root element was not found.');
}

createRoot(rootElement).render(<App document={JSON.parse(dataElement.textContent)} />);

import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  define: {
    'process.env.NODE_ENV': JSON.stringify('production'),
  },
  build: {
    outDir: '../Resources/dist',
    emptyOutDir: true,
    lib: {
      entry: 'src/main.tsx',
      name: 'IfrHtmlViewer',
      formats: ['iife'],
      fileName: () => 'viewer.js',
    },
    rollupOptions: {
      output: {
        assetFileNames: asset => asset.name?.endsWith('.css') ? 'viewer.css' : '[name][extname]',
      },
    },
  },
});

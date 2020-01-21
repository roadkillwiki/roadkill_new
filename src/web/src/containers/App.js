import React from "react";

import ComponentExample from "../components/ComponentExample";
import FunctionalComponent from "../components/FunctionalComponent";
import PageComponent from "../components/PageComponent";

import './App.css';

function App() {
  return (
    <div className="App">
      <header className="App-header">
        Not much here for now. Some components:

      <PageComponent />
      <hr />
      <ComponentExample />
      <FunctionalComponent />

      </header>
    </div>
  );
}

export default App;

import React from "react";

import ComponentExample from "../components/ComponentExample";
import FunctionalComponent from "../components/FunctionalComponent";
import PageComponent from "../components/PageComponent";

import './App.css';
import * as RoadkillApi from "roadkill-api-js";

function App() {

  var defaultClient = RoadkillApi.ApiClient.instance;
  defaultClient.basePath = "http://localhost:5001";

  let apiInstance = new RoadkillApi.AuthorizationApi(defaultClient);
  let authorizationRequest = new RoadkillApi.AuthorizationRequest();
  authorizationRequest.email = "admin@example.org";
  authorizationRequest.password = "Password1234567890";
  apiInstance.authorizationAuthenticate(authorizationRequest, (error, data, response) => {
  if (error) {
    console.error(error);
  } else {
    console.log('API called successfully. Returned data: ' + data);
  }
  });

  return (
    <div className="App">
        Not much here for now. Some components:
        <PageComponent />
        <ComponentExample />
        <FunctionalComponent />
    </div>
  );
}

export default App;

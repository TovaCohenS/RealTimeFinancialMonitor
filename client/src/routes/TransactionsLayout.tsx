import { useEffect } from "react";
import { Outlet } from "react-router-dom";
import  transactionsHubService  from "../services/signalRService";


export function TransactionsLayout() {
    useEffect(() => {       
       transactionsHubService.start();

    }, []);


    return <Outlet />;
}

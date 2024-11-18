import axios from 'axios';
import { API_URL } from '../environment/constants.js'

const httpClient = axios.create({
    baseURL: API_URL,
    withCredentials: true,
    timeout: 5000,
    headers: {'Content-Type': 'application/json',}
});

export const get = (endpoint, params) => {
    return httpClient.get(endpoint, { headers: params.headers, data: params.data});
}

export const post = (endpoint, data) => {
    return httpClient.post(endpoint, data);
}

export const del = (endpoint, data) => {
    return httpClient.delete(endpoint, {data: data });
}
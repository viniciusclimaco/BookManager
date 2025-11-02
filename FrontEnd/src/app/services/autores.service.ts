import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AutorDto, CreateAutorDto, UpdateAutorDto } from '../models/models';
import { environment } from '../config/api.config';

@Injectable({
  providedIn: 'root'
})
export class AutoresService {
  private apiUrl = `${environment.apiUrl}/Autores`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<AutorDto[]> {
    return this.http.get<AutorDto[]>(this.apiUrl);
  }

  getAtivos(): Observable<AutorDto[]> {
    return this.http.get<AutorDto[]>(`${this.apiUrl}/ativos`);
  }

  getById(id: number): Observable<AutorDto> {
    return this.http.get<AutorDto>(`${this.apiUrl}/${id}`);
  }

  create(autor: CreateAutorDto): Observable<number> {
    return this.http.post<number>(this.apiUrl, autor);
  }

  update(id: number, autor: UpdateAutorDto): Observable<boolean> {
    return this.http.put<boolean>(`${this.apiUrl}/${id}`, autor);
  }

  delete(id: number): Observable<boolean> {
    return this.http.delete<boolean>(`${this.apiUrl}/${id}`);
  }
}

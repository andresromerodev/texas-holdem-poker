﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Servidor
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Juego
    {
        [JsonProperty]
        public Stack<Carta> Mazo { get; set; }

        [JsonProperty]
        public List<Carta> CartasComunes { get; set; }

        [JsonProperty]
        public List<Jugador> Jugadores { get; set; }

        [JsonProperty]
        public int ApuestaAlta { get; set; }

        [JsonProperty]
        public int ApuestaMinima { get; set; }

        [JsonProperty]
        public int Bote { get; set; }

        [JsonProperty]
        public int Turno { get; set; }

        [JsonProperty]
        public int Ronda { get; set; }

        [JsonProperty]
        public string Informacion { get; set; }

        public Juego()
        {
            LlenarMazo();
            CartasComunes = new List<Carta>();
            Jugadores = new List<Jugador>();
            Turno = 0;
            Ronda = 0;
            Informacion = "";
        }

        public void LlenarMazo()
        {
            string[] leyendas = new string[] { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
            List<Carta> cartas = new List<Carta>();

            for (int i = 0; i < 4; i++)
            {
                for (int p = 0; p < 13; p++)
                {
                    cartas.Add(new Carta(leyendas[p], i));
                }
            }

            Barajar(cartas);
            Mazo = new Stack<Carta>(cartas);
        }

        public static void Barajar(List<Carta> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Carta value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

        }

        public void SacarFlop()
        {
            for (int i = 0; i < 3; i++)
            {
                CartasComunes.Add(Mazo.Pop());
            }

            Ronda = 2;
        }

        public void SacarTurn()
        {
            CartasComunes.Add(Mazo.Pop());
            Ronda = 3;
        }

        public void SacarRiver()
        {
            CartasComunes.Add(Mazo.Pop());
            Ronda = 4;
        }

        public void Repartir()
        {
            foreach (Jugador jugador in Jugadores)
            {
                jugador.Mano[0] = Mazo.Pop();
                jugador.Mano[1] = Mazo.Pop();
            }

            Ronda = 1;
        }

        public void ActualizarInformacion(string mensaje)
        {
            Informacion += mensaje;
            Console.WriteLine(mensaje);
        }

        public void DefinirApuestas() // Define los jugadores con apuestas y hace su resta
        {
            DefinirJugadorApuestaBaja();
            DefinirJugadorApuestaAlta();
        }

        public void DefinirJugadorApuestaBaja()
        {
            for (int i = 0; i < Jugadores.Count; i++)
            {
                if (Jugadores[i].Role.Equals(Jugador.APUESTA_BAJA))
                {
                    Jugadores[i].Role = Jugador.REGULAR;

                    if (i == (Jugadores.Count - 1)) // Si es el ultimo
                    {
                        Jugadores[0].Role = Jugador.APUESTA_BAJA;
                        Jugadores[0].ApuestaActual = ApuestaMinima;
                        Jugadores[0].CantFichas -= ApuestaMinima;
                    }
                }

                break;
            }
        }

        public void DefinirJugadorApuestaAlta()
        {
            for (int i = 0; i < Jugadores.Count; i++)
            {
                if (Jugadores[i].Role.Equals(Jugador.APUESTA_BAJA))
                {
                    if (i == (Jugadores.Count - 1)) // Si es el ultimo
                    {
                        Jugadores[0].Role = Jugador.APUESTA_ALTA;
                        Jugadores[0].ApuestaActual = ApuestaAlta;
                        Jugadores[0].CantFichas -= ApuestaAlta;
                    }
                    else
                    {
                        Jugadores[i + 1].Role = Jugador.APUESTA_ALTA;
                        Jugadores[0].ApuestaActual = ApuestaAlta;
                        Jugadores[0].CantFichas -= ApuestaAlta;
                    }
                }

                break;
            }
        }

        public Jugador EncontrarGanador()
        {
            cartaAlta();
            foreach (Jugador jugador in Jugadores)
            {
                par(jugador);
                doblePar(jugador);
                trio(jugador);
                escalera(jugador);

                // SI ESTAN:
                color(jugador);
                fullHouse(jugador);
                poker(jugador);
                escaleraDeColor(jugador);
                escaleraReal(jugador);
            }

            int ganador = 0;
            int puntajeMayor = 0;

            for (int i = 0; i < Jugadores.Count; i++)
            {
                if (Jugadores[i].PuntajeMano > puntajeMayor)
                {
                    puntajeMayor = Jugadores[i].PuntajeMano;
                    ganador = i;
                }
            }

            return Jugadores[ganador];
        }

        /// Estefany
        public int encontrarCartaMayor(Jugador jugador)
        {
            int mayor = 0;
            foreach (Carta carta in jugador.Mano)
            {
                int newMayor = carta.getValor();
                if (newMayor > mayor)
                {
                    mayor = newMayor;
                }
            }
            return mayor;
        }
        public void cartaAlta()
        {
            Jugador tieneMayor = new Jugador();
            int Mayor = 0;
            foreach (Jugador jugador in Jugadores)
            {
                int newMayor = encontrarCartaMayor(jugador);
                if (newMayor > Mayor)
                {
                    tieneMayor = jugador;
                    Mayor = newMayor;
                }
            }
            tieneMayor.PuntajeMano = 1;
        }
        public void par(Jugador jugador)
        {
            bool existePar = false;
            Carta actual = new Carta();
            int pos = 0;
            Carta carta1 = jugador.Mano[0];
            Carta carta2 = jugador.Mano[1];
            foreach (Carta carta in CartasComunes) {
                if (carta.getValor()==carta1.getValor() || carta.getValor()==carta2.getValor()) {
                    existePar=true;
                }
            }
            if (existePar)
            {
                jugador.PuntajeMano = 2;
            }
        }
        public void doblePar(Jugador jugador)
        {
            bool existe1par = false;
            bool existe2par = false;
            int pos = 0;
            Carta carta1 = jugador.Mano[0];
            Carta carta2 = jugador.Mano[1];
            foreach (Carta carta in CartasComunes)
            {
                if (carta.getValor() == carta1.getValor())
                {
                    existe1par = true;
                }
            }
            foreach (Carta carta in CartasComunes)
            {
                if (carta.getValor() == carta2.getValor())
                {
                    existe2par=true;
                }
            }

            if (existe1par && existe2par)
            {
                jugador.PuntajeMano = 3;
            }
        }
        public void trio(Jugador jugador)
        {
            int contador = 0;
            bool existe3del1 = false;
            bool existe3del2 = false;
            int pos = 0;
            Carta carta1 = jugador.Mano[0];
            Carta carta2 = jugador.Mano[1];
            foreach (Carta carta in CartasComunes)
            {
                if (carta.getValor() == carta1.getValor())
                {
                    contador++;
                }
            }
            
            if (contador == 2)
            {
                existe3del1 = true;
            }
            contador = 0;
            foreach (Carta carta in CartasComunes)
            {
                if (carta.getValor() == carta2.getValor())
                {
                    contador++;
                }
            }

            if (contador == 2)
            {
                existe3del2 = true;
            }

            if (existe3del1 || existe3del2) {
                jugador.PuntajeMano = 4;
            }
        }
        public void escalera(Jugador jugador)
        {
            //Ordenar Mano
            Carta[] cartas = new Carta[5];
            cartas[0] = jugador.Mano[0];
            cartas[1] = jugador.Mano[1];
            int pos = 2;
            foreach (Carta carta in CartasComunes)
            {
                if (pos<5) {
                    cartas[pos] = carta;
                    pos++; }
            }
            for (int x = 0; x < cartas.Length; x++)
            {
                for (int i = 0; i < cartas.Length - x - 1; i++)
                {
                    if (cartas[i].getValor() < cartas[i + 1].getValor())
                    {
                        int tmp = cartas[i + 1].getValor();
                        cartas[i + 1] = cartas[i];
                        cartas[i].Leyenda = tmp.ToString();
                    }
                }
            }
            bool existeEscalera = true;
            int cantidadCartas = 0;
            for (int i = 1; i < cartas.Length; i++)
            {
                if ((cartas[i].getValor() - 1) != cartas[i - 1].getValor())
                {
                    existeEscalera = false;
                }
                if (cantidadCartas<5) {
                    cantidadCartas++;
                }
            }
            if (existeEscalera && cantidadCartas==5)
            {
                jugador.PuntajeMano = 5;
            }
        }

        ///Ariel
        public void color(Jugador jugador)
        {
            if (jugador.Mano[0].TipoPalo == jugador.Mano[1].TipoPalo)
            {
                int tipopalomano = jugador.Mano[0].TipoPalo;
                int contador = 0;
                foreach (Carta c in CartasComunes)
                {
                    if (c.TipoPalo == tipopalomano)
                    {
                        contador += 1;
                    }
                }
                jugador.PuntajeMano = 6;
            }
        }
        public void fullHouse(Jugador jugador)
        {
            int contador1 = 0;
            string valor1 = jugador.Mano[0].Leyenda;
            int contador2 = 0;
            string valor2 = jugador.Mano[1].Leyenda;

            if (valor1.Equals(valor2))
            {
                contador1 = 2;
                foreach (Carta c in CartasComunes)
                {
                    if (c.Leyenda != valor1)
                    {
                        valor2 = c.Leyenda;
                        break;
                    }
                }
                foreach (Carta c in CartasComunes)
                {
                    if (c.Leyenda == valor1)
                    {
                        contador1 += 1;
                    }
                    else if (c.Leyenda == valor2)
                    {
                        contador2 += 1;
                    }
                }
            }
            else
            {
                contador1 = 1;
                contador2 = 2;
                foreach (Carta c in CartasComunes)
                {
                    if (c.Leyenda == valor1)
                    {
                        contador1 += 1;
                    }
                    else if (c.Leyenda == valor2)
                    {
                        contador2 += 1;
                    }
                }
            }
            if (contador1 == 2 && contador2 == 3 || contador1 == 3 && contador2 == 2)
                jugador.PuntajeMano = 7;
        }
        public void poker(Jugador jugador)
        {
            int contador1 = 0;
            string valor1 = jugador.Mano[0].Leyenda;
            string valor2 = jugador.Mano[1].Leyenda;
            int contador2 = 0;

            if (valor1.Equals(valor2))
            {
                foreach (Carta c in CartasComunes)
                {
                    contador1 = 2;
                    if (c.Leyenda == valor1)
                    {
                        contador1 += 1;
                    }
                }
            }
            else
            {
                contador1 = 1;
                contador2 = 1;
                foreach (Carta c in CartasComunes)
                {
                    if (c.Leyenda == valor1)
                    {
                        contador1 += 1;
                    }
                    else if (c.Leyenda == valor2)
                    {
                        contador2 += 1;
                    }
                }
            }
            if (contador1 == 4 || contador2 == 4)
                jugador.PuntajeMano = 8;
        }
        public void escaleraDeColor(Jugador jugador)
        {
            int contador = 0;
            if (!jugador.Mano[0].Leyenda.Equals("A") && !jugador.Mano[1].Leyenda.Equals("A") && jugador.Mano[0].TipoPalo == jugador.Mano[1].TipoPalo)
            {
                contador = 2;
                foreach (Carta c in CartasComunes)
                {
                    if (c.TipoPalo == jugador.Mano[0].TipoPalo && !jugador.Mano[1].Leyenda.Equals("A"))
                        contador += 1;
                }
            }
            if (contador >= 5)
                jugador.PuntajeMano = 9;
        }
        public void escaleraReal(Jugador jugador)
        {
            int contador = 0;
            string[] leyendas = new string[] { "A", "10", "J", "Q", "K" };
            if (!jugador.Mano[0].Leyenda.Equals(jugador.Mano[1].Leyenda) && jugador.Mano[0].TipoPalo == jugador.Mano[1].TipoPalo)
            {
                for (int i = 0; i < leyendas.Length; i++)
                {
                    if (jugador.Mano[0].Leyenda.Equals(leyendas[i]))
                    {
                        contador += 1;
                        leyendas[i] = "";
                    }
                    else if (jugador.Mano[1].Leyenda.Equals(leyendas[i]))
                    {
                        contador += 1;
                        leyendas[i] = "";
                    }
                }
                if (contador >= 2)
                {
                    foreach (Carta c in CartasComunes)
                    {
                        if (c.TipoPalo == jugador.Mano[0].TipoPalo)
                            for (int i = 0; i < leyendas.Length; i++)
                            {
                                if (c.Leyenda.Equals(leyendas[i]))
                                {
                                    contador += 1;
                                    leyendas[i] = "";
                                }
                            }
                    }
                }
            }
            if (contador >= 5)
                jugador.PuntajeMano = 10;
        }
    }
}

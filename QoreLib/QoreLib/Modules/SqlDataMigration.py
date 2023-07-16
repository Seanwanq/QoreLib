import numpy as np
import sqlite3
import datetime
import os
import gzip
import pickle
import json
import copy
import posixpath
originalDbBaseUrl = "E:/quantum/data"
dbUrl = os.path.join(originalDbBaseUrl, "waveforms.db")
connection = sqlite3.connect(dbUrl)
cursor = connection.cursor()
cursor.execute("SELECT id, app, file FROM records WHERE app = 'Spectrum' AND file LIKE 'objects%'")
results = cursor.fetchall()
cursor.close()
connection.close()

def LoadObject(filePath: str):
    with gzip.open(filePath, 'rb') as f:
        data = pickle.load(f)
    return data

def MakeJson(obj, basePath):
    idPath = str(obj["GroupId"])
    namePath = obj["Name"]
    fullPath = os.path.join(basePath, "DataFiles", idPath, namePath, "BaseData.json")
    relativePath = posixpath.join("DataFiles", idPath, namePath, "BaseData.json")
    dirPath = os.path.dirname(fullPath)
    if not os.path.exists(dirPath):
        os.makedirs(dirPath)
    with open(fullPath, 'w', encoding='utf-8') as f:
        json.dump(obj, f, ensure_ascii=False, indent=4)
    return relativePath

def LoadDataAndSaveToSql(idValue, result, typeValue, basePath, destinationDbCursor):
    dataIndexAll = result['index']
    length = len(dataIndexAll)
    dataKeys = list(dataIndexAll.keys())
    for i in range(0, length):
        groupId = copy.deepcopy(idValue)
        name = dataKeys[i]
        dataIndex = dataIndexAll[dataKeys[i]].tolist()
        dataValue = result[typeValue][:, i].tolist()
        dataValueReal = [c.real for c in dataValue]
        dataValueImagine = [c.imag for c in dataValue]
        dataDict = {
            "GroupId": groupId,
            "Name": name,
            "ValueType": typeValue,
            "Index": dataIndex,
            "ValueReal": dataValueReal,
            "ValueImagine": dataValueImagine
        }
        dataFileRelativePath = MakeJson(dataDict, basePath)
        app = "Spectrum"
        type = typeValue
        isFilled = False
        createTime = datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S')
        alterTime = copy.copy(createTime)
        existingDataQuery = f"SELECT * FROM SpectrumTable WHERE GroupId = {groupId} AND Name = '{name}'"
        destinationDbCursor.execute(existingDataQuery)
        existingData = destinationDbCursor.fetchone()

        if existingData is None:
            insertQuery = f"INSERT INTO SpectrumTable (GroupId, DataFile, APP, Type, Name, IsFilled, CreateTime, AlterTime) VALUES ({groupId}, '{dataFileRelativePath}', '{app}', '{type}', '{name}', {isFilled}, '{createTime}', '{alterTime}')"
            destinationDbCursor.execute(insertQuery)



destinationDbBasePath = "D:/qorelibdata"
destinationDbFilePath = os.path.join(destinationDbBasePath, "coredata.db")
connectionToDestinationDb = sqlite3.connect(destinationDbFilePath)
destinationDbCursor = connectionToDestinationDb.cursor()

badRows = []
for row in results:
    idValue = row[0]
    appValue = row[1]
    fileValue = row[2]
    filePath = os.path.join(originalDbBaseUrl, fileValue)
    try:
        result = LoadObject(filePath)
        if 'remote_iq_avg' in result and 'index' in result:
            typeValue = 'remote_iq_avg'
            LoadDataAndSaveToSql(idValue, result, typeValue, destinationDbBasePath, destinationDbCursor)
            pass
        elif 'iq_avg' in result and 'index' in result:
            typeValue = 'iq_avg'
            LoadDataAndSaveToSql(idValue, result, typeValue, destinationDbBasePath, destinationDbCursor)
            pass
        elif 'remote_population' in result and 'index' in result:
            typeValue = 'remote_population'
            LoadDataAndSaveToSql(idValue, result, typeValue, destinationDbBasePath, destinationDbCursor)
            pass
        else:
            badRows.append(row)
            pass
        pass
    except FileNotFoundError:
        badRows.append(row)

for badRow in badRows:
    results.remove(badRow)

connectionToDestinationDb.commit()
destinationDbCursor.close()
connectionToDestinationDb.close()

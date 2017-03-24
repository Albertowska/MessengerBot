<?php

function generateRandomString($length = 10) {
    $characters = '0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ';
    $charactersLength = strlen($characters);
    $randomString = '';
    for ($i = 0; $i < $length; $i++) {
        $randomString .= $characters[rand(0, $charactersLength - 1)];
    }
    return $randomString;
}


$id = $_GET['idImage'];
//$id = 12;
//$url = 'http://www.eina.es/images_bd/estenea-project-study-of-low-cost-technologies-and-high-cadences-in-composites000155.jpg';
//var_dump($url);
$url = "http://localhost:8080/image/" . $id;
$random = generateRandomString();
$timeStamp = time();
$name = $random.$timeStamp.".jpg";
$uploadfile = '/var/www/html/uploads/' . $name;
//var_dump($uploadfile);
copy($url, $uploadfile);
//$uploadfile = '/var/www/html/uploads/2.jpg';
//move_uploaded_file($_FILES['image']['tmp_name'], $uploadfile);

$result = shell_exec("cd /var/www/html/tensorflow; sudo bazel-bin/tensorflow/examples/label_image/label_image --graph=/tmp/output_graph.pb --labels=/tmp/output_labels.txt --output_layer=final_result --image=" . $uploadfile . " 2>&1 ");

$separator = "\n";
$lineArray = explode($separator, $result);

$result = array('name' => '');

foreach ($lineArray as $line)
{
    if(strpos($line, 'tensorflow/examples/label_image/main.cc:206') != false)
    {
         $data = preg_split("/]/", $line);
         $name = preg_split("[\s]", trim($data[1]))[0];
         $score = preg_split("[\s]", trim($data[1]))[2];
         echo(json_encode(array('name' => $name)));
	 exit();
    }
}

//echo(json_encode($result));

?>
